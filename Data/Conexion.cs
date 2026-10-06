using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace RestauranteMonster.Data;

public static class Conexion
{
    private static IMongoDatabase? _db;

    static Conexion()
    {
        // Los documentos de Mongo usan camelCase (nombre, idRol, idPlato, ...)
        // mientras que las clases C# usan PascalCase; esta convención alinea ambos.
        ConventionRegistry.Register("camelCase", new ConventionPack { new CamelCaseElementNameConvention() }, _ => true);
    }

    public static void Configure(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _db = client.GetDatabase(databaseName);
    }

    public static IMongoDatabase GetDatabase() =>
        _db ?? throw new InvalidOperationException("Conexion no configurada. Llame a Conexion.Configure primero.");

    public static IMongoCollection<T> GetCollection<T>(string nombre) => GetDatabase().GetCollection<T>(nombre);

    /// <summary>Genera IDs enteros autoincrementales por colección, imitando IDENTITY de SQL Server.</summary>
    public static int SiguienteId(string coleccion)
    {
        var contadores = GetDatabase().GetCollection<BsonDocument>("contadores");
        var filtro = Builders<BsonDocument>.Filter.Eq("_id", coleccion);
        var actualizacion = Builders<BsonDocument>.Update.Inc("secuencia", 1);
        var opciones = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        var resultado = contadores.FindOneAndUpdate(filtro, actualizacion, opciones);
        return resultado["secuencia"].AsInt32;
    }
}
