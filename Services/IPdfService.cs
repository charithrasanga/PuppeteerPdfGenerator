
using System.Threading.Tasks;

public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(GeneratePdfRequest request);
}