namespace CustomPage.HangfireExtensions.ManagementPage
{
    public delegate string BuildInsertStatement(object row);

    public class TableConfiguration
    {
        public string Name { get; set; }
        public string[] Fields { get; set; }
        public BuildInsertStatement InsertBuilder { get; set; }
    }
}
