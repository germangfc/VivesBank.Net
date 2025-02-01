using Microsoft.Extensions.FileProviders;

namespace VivesBankApi.Rest.Product.Base.Storage;

public interface IStorageCsv
{
    List<Base.Models.Product> LoadCsv(Stream stream);
}