using Appwrite;
using Appwrite.Services;

namespace SmartBlaze.Backend.Repositories;

public abstract class AbstractRepository
{
    protected const string AppwriteDatabaseId = "667422240010ed7f3097";
    protected const string ChatSessionCollectionId = "6685332600286e5a6976";
    protected const string MessageCollectionId = "6685339500109e79a0de";
    protected const string ChatbotConfigurationCollectionId = "66952f890014e29c653d";
    
    private const string AppwriteEndpoint = "https://cloud.appwrite.io/v1";
    private const string AppwriteProjectId = "6674214b0022089a10cb";
    private const string AppwriteApiKey =
        "0021d6b1bfcebfcce2de48ca7112a6f55ad0f8b0994ed93a383e3bec99506485f318e90a37635" +
        "025c85058bd342a416bb45095f926912bfdf94923d581d7e70af600204acfc908c11f5da1bf69d" +
        "785d900d440c12d7005c332a98818641a0a0717f13979ccd38420a24565e6909cc6db09a3f200755198838ccd667e0158dd58";

    private readonly Client _appwriteClient;
    private readonly Databases _appwriteDatabase;
    

    protected AbstractRepository()
    {
        _appwriteClient = new Client();
        SetUpClient();

        _appwriteDatabase = new Databases(_appwriteClient);
    }
    
    public Databases AppwriteDatabase => _appwriteDatabase;

    private void SetUpClient()
    {
        _appwriteClient
            .SetEndpoint(AppwriteEndpoint)
            .SetProject(AppwriteProjectId)
            .SetKey(AppwriteApiKey);
    }
}