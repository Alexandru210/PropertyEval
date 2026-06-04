namespace PropertyEval.Application.DTOs;

public class GetPropertyValuationRequest
{
    public int Id { get; set; }
}

public record PropertyValuationResponse(
    int PropertyId,
    decimal PredictedValue,
    decimal PredictionLowerBound,
    decimal PredictionUpperBound,
    int TrainingRowCount,
    double RSquared,
    double MeanAbsoluteError,
    double RootMeanSquaredError,
    string ModelName
);
