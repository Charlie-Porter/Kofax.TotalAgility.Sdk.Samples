namespace Kofax.TotalAgility.Sdk.Samples.CaptureDocumentService
{
    /// <summary>
    /// Return object used by CaptureDocumentServiceSample CreateFolders method for storing folder ids.
    /// </summary>
    public class CreatedFolders
    {
        /// <summary>
        /// Root folder’s id.
        /// </summary>
        public string RootFolderId { get; set; }

        /// <summary>
        /// Child folder 1’s id
        /// </summary>
        public string ChildFolder1Id { get; set; }

        /// <summary>
        /// Child folder 2’s id
        /// </summary>
        public string ChildFolder2Id { get; set; }

        /// <summary>
        /// Child folder 3’s id
        /// </summary>
        public string ChildFolder3Id { get; set; }

        /// <summary>
        /// Grand child folder 1’s id
        /// </summary>
        public string GrandChildFolder1Id { get; set; }
    }
}
