using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Common;
using PropertyEval.Domain.Entities;
using PropertyEval.Domain.Enums;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services;

public class PropertyValuationService
{
    private const int MinimumTrainingRows = 5;
    private const string ModelName = "ML.NET FastTree regression";

    private readonly AppDbContext _context;

    public PropertyValuationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyValuationResponse> PredictPropertyValueAsync(
        int propertyId,
        int userId,
        bool canValueAnyProperty,
        CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Address)
            .Include(p => p.Listings)
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (property is null)
        {
            throw new KeyNotFoundException("Property was not found.");
        }

        if (!canValueAnyProperty && property.OwnerUserId != userId && !HasActiveListing(property))
        {
            throw new ForbiddenAccessException("You can only value public listings or properties you created.");
        }

        return await PredictPropertyValueAsync(property, cancellationToken);
    }

    public async Task<PropertyValuationResponse> PredictPropertyValueAsync(Property property, CancellationToken cancellationToken)
    {
        var trainingRows = await LoadTrainingRowsAsync(property.Id, cancellationToken);

        if (trainingRows.Count < MinimumTrainingRows)
        {
            throw new InvalidOperationException(
                $"At least {MinimumTrainingRows} completed evaluations or listings with positive prices are required to train the valuation model.");
        }

        var mlContext = new MLContext(seed: 1);
        var dataView = mlContext.Data.LoadFromEnumerable(trainingRows);
        var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 1);

        var pipeline = BuildPipeline(mlContext);
        var model = pipeline.Fit(split.TrainSet);

        var predictions = model.Transform(split.TestSet);
        var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

        using var predictionEngine = mlContext.Model.CreatePredictionEngine<PropertyValuationModelInput, PropertyValuationModelOutput>(model);
        var output = predictionEngine.Predict(ToModelInput(property));

        var predictedValue = ToMoney(output.PredictedValue);
        var errorMargin = ToMoney(CleanMetric(metrics.RootMeanSquaredError));

        return new PropertyValuationResponse(
            property.Id,
            predictedValue,
            Math.Max(0m, predictedValue - errorMargin),
            predictedValue + errorMargin,
            trainingRows.Count,
            CleanMetric(metrics.RSquared),
            CleanMetric(metrics.MeanAbsoluteError),
            CleanMetric(metrics.RootMeanSquaredError),
            ModelName
        );
    }

    private static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(
                outputColumnName: "PropertyTypeEncoded",
                inputColumnName: nameof(PropertyValuationModelInput.PropertyType))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                outputColumnName: "CityEncoded",
                inputColumnName: nameof(PropertyValuationModelInput.City)))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                outputColumnName: "CountyEncoded",
                inputColumnName: nameof(PropertyValuationModelInput.County)))
            .Append(mlContext.Transforms.Concatenate(
                "Features",
                nameof(PropertyValuationModelInput.Area),
                nameof(PropertyValuationModelInput.Bedrooms),
                nameof(PropertyValuationModelInput.Bathrooms),
                nameof(PropertyValuationModelInput.YearBuilt),
                "PropertyTypeEncoded",
                "CityEncoded",
                "CountyEncoded"))
            .Append(mlContext.Regression.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 1));
    }

    private async Task<IReadOnlyList<PropertyValuationModelInput>> LoadTrainingRowsAsync(
        int excludedPropertyId,
        CancellationToken cancellationToken)
    {
        var evaluations = await _context.Evaluations
            .Include(e => e.Property)
            .ThenInclude(p => p.Address)
            .AsNoTracking()
            .Where(e => e.PropertyId != excludedPropertyId)
            .Where(e => e.Status == EvaluationStatus.Completed)
            .Where(e => e.EvaluatedValue > 0m)
            .ToListAsync(cancellationToken);

        var rows = evaluations
            .Select(e => ToModelInput(e.Property, e.EvaluatedValue))
            .ToList();

        var listings = await _context.Listings
            .Include(l => l.Property)
            .ThenInclude(p => p.Address)
            .AsNoTracking()
            .Where(l => l.PropertyId != excludedPropertyId)
            .Where(l => l.AskingPrice > 0m)
            .ToListAsync(cancellationToken);

        rows.AddRange(listings.Select(l => ToModelInput(l.Property, l.AskingPrice)));

        return rows;
    }

    private static PropertyValuationModelInput ToModelInput(Property property, decimal label = 0m)
    {
        return new PropertyValuationModelInput
        {
            Area = property.Area,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            YearBuilt = property.YearBuilt,
            PropertyType = property.PropertyType.ToString(),
            City = property.Address.City,
            County = property.Address.County,
            Label = (float)label
        };
    }

    private static decimal ToMoney(float value)
    {
        return Math.Round((decimal)Math.Max(0f, value), 2);
    }

    private static decimal ToMoney(double value)
    {
        return Math.Round((decimal)Math.Max(0d, value), 2);
    }

    private static double CleanMetric(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value) ? 0d : value;
    }

    private static bool HasActiveListing(Property property)
    {
        return property.Listings.Any(l => l.Status == ListingStatus.Active);
    }
}

internal sealed class PropertyValuationModelInput
{
    public float Area { get; set; }
    public float Bedrooms { get; set; }
    public float Bathrooms { get; set; }
    public float YearBuilt { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public float Label { get; set; }
}

internal sealed class PropertyValuationModelOutput
{
    [ColumnName("Score")]
    public float PredictedValue { get; set; }
}
