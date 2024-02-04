namespace Elasticsearch.API.DTOs
{
    //Record -- nesne örneği üretildikten sonra propertyleri değiştirilemiyor.
    public record ProductUpdateDto(string Id,string Name, decimal Price, int Stock, ProductFeatureDto Feature)
    {
    }
}
