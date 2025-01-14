namespace VivesBankApi.Rest.Movimientos.Config;

public class MongoDatabaseConfig
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string DomiciliacionCollectionName { get; set; } = null!;
    
    public string MovimientosCollectionName { get; set; } = null!;
}