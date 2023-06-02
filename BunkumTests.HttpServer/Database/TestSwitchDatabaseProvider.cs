using Bunkum.HttpServer.Database;

namespace BunkumTests.HttpServer.Database;

public class TestSwitchDatabaseProvider : IDatabaseProvider<TestSwitchDatabaseContext>
{
    public void Initialize()
    {
        
    }

    public void Warmup()
    {
        
    }

    public TestSwitchDatabaseContext GetContext()
    {
        return new TestSwitchDatabaseContext();
    }
    
    public void Dispose() {}
}