using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Agility.Sdk.Model.Capture;
using Agility.Sdk.Model.Capture.Validation;
using Agility.Sdk.Model.Categories;
using Agility.Sdk.Model.Server;
using sdk = TotalAgility.Sdk;

namespace Kofax.TotalAgility.Sdk.Samples.CaptureDocumentService
{
    /// <summary>
    /// This class contains sample methods that call corresponding CaptureDocumentService API methods to
    /// demonstrate usage of the API methods. The Capture SDK Sample Package contains processes
    /// that call these methods.
    /// </summary>
    public class CaptureDocumentServiceSample
    {
        #region Create Methods

        /// <summary>
        /// This method creates one root folder, and then three child folders. A grandchild folder is added
        /// to the second child folder.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <returns>The created folders' ids.</returns>
        public CreatedFolders CreateFoldersSample(string sessionId)
        {
            var createdFolders = new CreatedFolders();
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Generate root folder. "SDKSample" is the name of the Folder Type that defines the folder fields.
                createdFolders.RootFolderId = cds.CreateFolder(sessionId, null, null, null, -1, null, new FolderTypeIdentity() { Name = "SDKSample" });

                // Add child folder 1 and initialize its Valid property.
                createdFolders.ChildFolder1Id = cds.CreateFolder(sessionId, null, createdFolders.RootFolderId, new RuntimeFieldCollection() {
                    new RuntimeField() {
                        Id = "5B83535412F142669762C8C08CFE690F",    // The Valid property
                        Value = true
                    }
                }, -1, null, null);

                // Add child folder 2.
                createdFolders.ChildFolder2Id = cds.CreateFolder(sessionId, null, createdFolders.RootFolderId, null, 1, null, null);

                // Add child folder 3.
                createdFolders.ChildFolder3Id = cds.CreateFolder(sessionId, null, createdFolders.RootFolderId, null, 2, null, null);

                // Add grand child folder 1.
                createdFolders.GrandChildFolder1Id = cds.CreateFolder(sessionId, null, createdFolders.ChildFolder2Id, null, -1, null, null);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdFolders;
        }

        /// <summary>
        /// This method creates two documents (document 1 and 2) with 5 pages each under the folder
        /// identified by parentId. For demonstration purposes, a third document is created under
        /// a root folder that is created when no parent folder is specified. At least one property of
        /// each of the documents is initialized and a document type is assigned.  The third document and
        /// its root folder is then deleted.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="parentId">Parent folder id.</param>
        /// <param name="filePath1">File path for document 1's pages.</param>
        /// <param name="filePath2">File path for document 2's pages.</param>
        /// <returns>The created documents' ids.</returns>
        public CreatedDocuments CreateDocumentsSample(string sessionId, string parentId, string filePath1, string filePath2)
        {
            var createdDocuments = new CreatedDocuments();
            var cds = new sdk.CaptureDocumentService();
            var cts = new sdk.CategoryService();
            var ccs = new sdk.CaptureConfigurationService();

            try
            {
                // Retrieve categories
                var categoryFilter = new CategoryFilter()
                {
                    CategoryLevel = 2,
                    CheckAccess = false,
                    ParentCategory = new CategoryIdentity()
                };

                var categories = cts.GetCategories(sessionId, categoryFilter);

                // The document types we want are in a classification group that is under the "SDK Samples" category.
                var samplesCategory = categories.First(c => c.Identity.Name == "SDK Samples");

                // Get the classification groups that are under the samples category.
                var classificationGroupFilter = new ClassificationGroupFilter()
                {
                    Category = new CategoryIdentity()
                    {
                        Id = samplesCategory.Identity.Id,
                        Name = samplesCategory.Identity.Name,
                        ExtensionData = samplesCategory.Identity.ExtensionData
                    }
                };

                var classificationGroups = ccs.GetClassificationGroups(sessionId, classificationGroupFilter);

                // Now filter out the classification group that we want.
                var samplesClassificationGroup = classificationGroups.First(cg => cg.Identity.Name == "SDK Samples Group");

                // Finally, get all document types in the classification group.
                var docTypes = ccs.GetDocumentTypes(sessionId, samplesClassificationGroup.Identity);

                // Get the Northwest Order Forms document type identifier.
                var northwestTypeId = docTypes.First(d => d.Name == "NW Form").Identity.Id;

                // Get the Tri-Spectrum Order Forms document type identifier.
                var triSpectrumTypeId = docTypes.First(d => d.Name == "TS Form").Identity.Id;

                // Add document 1 and set the type to NW Form
                createdDocuments.Document1Id = cds.CreateDocument(sessionId, null, parentId, new RuntimeFieldCollection() {
                    new RuntimeField() {
                        Id = "5B83535412F142669762C8C08CFE690F",    // Valid property
                        Value = true
                    }
                }, filePath1, 0, northwestTypeId, null).DocumentId;

                // Add document 2 and set the type to TS Form
                createdDocuments.Document2Id = cds.CreateDocument(sessionId, null, parentId, null, filePath2, 1, triSpectrumTypeId, null).DocumentId;

                // Create document and parent folder in single call.
                var folderId = cds.CreateDocument(sessionId, null, null, null, filePath1, 0, null, null).FolderId;

                // Delete this new folder and document.
                cds.DeleteFolder(sessionId, folderId, null, false);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdDocuments;
        }

        /// <summary>
        /// It creates two documents under the folder identified by parentId. The document types, and two field values
        /// and properties are set on the new documents. It returns an array of strings containing the document ids of
        /// the new documents.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="parentId">Parent folder id.</param>
        /// <returns>The created documents' ids.</returns>
        public CreatedDocuments CreateDocuments2Sample(string sessionId, string parentId)
        {
            var createdDocuments = new CreatedDocuments();
            var cds = new sdk.CaptureDocumentService();
            var cts = new sdk.CategoryService();
            var ccs = new sdk.CaptureConfigurationService();

            try
            {
                // Retrieve categories
                var categoryFilter = new CategoryFilter()
                {
                    CategoryLevel = 2,
                    CheckAccess = false,
                    ParentCategory = new CategoryIdentity()
                };

                var categories = cts.GetCategories(sessionId, categoryFilter);

                // The document types we want are in a classification group that is under the "SDK Samples" category.
                var samplesCategory = categories.First(c => c.Identity.Name == "SDK Samples");

                // Get the classification groups that are under the samples category.
                var classificationGroupFilter = new ClassificationGroupFilter()
                {
                    Category = new CategoryIdentity()
                    {
                        Id = samplesCategory.Identity.Id,
                        Name = samplesCategory.Identity.Name,
                        ExtensionData = samplesCategory.Identity.ExtensionData
                    }
                };

                var classificationGroups = ccs.GetClassificationGroups(sessionId, classificationGroupFilter);

                // Now filter out the classification group that we want.
                var samplesClassificationGroup = classificationGroups.First(cg => cg.Identity.Name == "SDK Samples Group");

                // Finally, get all document types in the classification group.
                var docTypes = ccs.GetDocumentTypes(sessionId, samplesClassificationGroup.Identity);

                // Get the Northwest Order Forms document type identifier.
                var northwestTypeId = docTypes.First(d => d.Name == "NW Form").Identity.Id;

                // Get the Tri-Spectrum Order Forms document type identifier.
                var triSpectrumTypeId = docTypes.First(d => d.Name == "TS Form").Identity.Id;

                // Create two documents.
                var docIds = cds.CreateDocuments2(sessionId, null, parentId, new RuntimeFieldCollection() {
                    new RuntimeField {
                        Id = "5B83535412F142669762C8C08CFE690F",    // Folder Valid property
                        Value = false
                    },
                    new RuntimeField {
                        Id = "D735014F88744E3899D6AB8EAA84634A",    // Folder ReviewValid property
                        Value = false
                    }
                }, null, new DocumentDataInput2Collection {
                    new DocumentDataInput2() {
                        DocumentTypeIdentity = new DocumentTypeIdentity() {
                            Id = northwestTypeId
                        },
                        RuntimeFields = new RuntimeFieldCollection() {
                            new RuntimeField() {
                                Id = "5B83535412F142669762C8C08CFE690F",    // Document Valid property
                                Value = true
                            },
                            new RuntimeField() {
                                Id = "D735014F88744E3899D6AB8EAA84634A",    // Document ReviewValid property
                                Value = false
                            }
                        },
                        RuntimeFieldProperties = new FieldPropertiesCollection {
                            new FieldProperties(new RuntimeFieldIdentity("CustomerName"), new FieldSystemPropertyCollection {
                                new FieldSystemProperty("0B8889DC16EA4F32AFFED1A48E49A720", "First and Last Name"), // Value
                                new FieldSystemProperty("57491C9612194E639DBF85ECD794ECB2", 150)    // Width
                            }),
                            new FieldProperties(new RuntimeFieldIdentity("Address"), new FieldSystemPropertyCollection {
                                new FieldSystemProperty("0B8889DC16EA4F32AFFED1A48E49A720", "Street City Zip"), // Value
                                new FieldSystemProperty("57491C9612194E639DBF85ECD794ECB2", 250)    // Width
                            })
                        }
                    },
                    new DocumentDataInput2() {
                        DocumentTypeIdentity = new DocumentTypeIdentity() {
                            Id = triSpectrumTypeId
                        },
                        RuntimeFields = new RuntimeFieldCollection() {
                            new RuntimeField {
                                Id = "5B83535412F142669762C8C08CFE690F",    // Document Valid property
                                Value = false
                            },
                            new RuntimeField {
                                Name = "D735014F88744E3899D6AB8EAA84634A",    // Document ReviewValid property
                                Value = false
                            }
                        },
                        RuntimeFieldProperties = new FieldPropertiesCollection {
                            new FieldProperties(new RuntimeFieldIdentity("CustomerName"), new FieldSystemPropertyCollection {
                                new FieldSystemProperty("0B8889DC16EA4F32AFFED1A48E49A720", "Another Name"), // Value
                                new FieldSystemProperty("57491C9612194E639DBF85ECD794ECB2", 175)    // Width
                            }),
                            new FieldProperties(new RuntimeFieldIdentity("Address"), new FieldSystemPropertyCollection {
                                new FieldSystemProperty("0B8889DC16EA4F32AFFED1A48E49A720", "Another Address"), // Value
                                new FieldSystemProperty("57491C9612194E639DBF85ECD794ECB2", 275)    // Width
                            })
                        }
                    }
                });

                createdDocuments.Document1Id = docIds[0].DocumentId;
                createdDocuments.Document2Id = docIds[1].DocumentId;
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdDocuments;
        }

        /// <summary>
        /// It creates a document under the folder identified by parentId. The document type, and two field values
        /// and properties are set on the new document. It returns the document Id of the new document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="parentId">Parent folder id.</param>
        /// <returns>The created document id.</returns>
        public string CreateDocument3Sample(string sessionId, string parentId)
        {
            string createdDocumentId = null;
            var cds = new sdk.CaptureDocumentService();
            var cts = new sdk.CategoryService();
            var ccs = new sdk.CaptureConfigurationService();

            try
            {
                // Retrieve categories
                var categoryFilter = new CategoryFilter()
                {
                    CategoryLevel = 2,
                    CheckAccess = false,
                    ParentCategory = new CategoryIdentity()
                };

                var categories = cts.GetCategories(sessionId, categoryFilter);

                // The document types we want are in a classification group that is under the "SDK Samples" category.
                var samplesCategory = categories.First(c => c.Identity.Name == "SDK Samples");

                // Get the classification groups that are under the samples category.
                var classificationGroupFilter = new ClassificationGroupFilter()
                {
                    Category = new CategoryIdentity()
                    {
                        Id = samplesCategory.Identity.Id,
                        Name = samplesCategory.Identity.Name,
                        ExtensionData = samplesCategory.Identity.ExtensionData
                    }
                };

                var classificationGroups = ccs.GetClassificationGroups(sessionId, classificationGroupFilter);

                // Now filter out the classification group that we want.
                var samplesClassificationGroup = classificationGroups.First(cg => cg.Identity.Name == "SDK Samples Group");

                // Finally, get all document types in the classification group.
                var docTypes = ccs.GetDocumentTypes(sessionId, samplesClassificationGroup.Identity);

                // Get the Northwest Order Forms document type identifier.
                var northwestTypeId = docTypes.First(d => d.Name == "NW Form").Identity.Id;

                // Create document.
                createdDocumentId = cds.CreateDocument3(sessionId, null, parentId, new RuntimeFieldCollection() {
                    new RuntimeField() {
                        Id = "5B83535412F142669762C8C08CFE690F",    // Folder Valid property
                        Value = false
                    },
                    new RuntimeField() {
                        Id = "D735014F88744E3899D6AB8EAA84634A",    // Folder ReviewValid property
                        Value = false
                    }
                }, null, new DocumentDataInput2() {
                        DocumentTypeIdentity = new DocumentTypeIdentity() {
                            Id = northwestTypeId
                        },
                        RuntimeFields = new RuntimeFieldCollection() {
                            new RuntimeField {
                                Id = "5B83535412F142669762C8C08CFE690F",    // Document Valid property
                                Value = true
                            },
                            new RuntimeField {
                                Id = "D735014F88744E3899D6AB8EAA84634A",    // Document ReviewValid property
                                Value = false
                            }
                        },
                        RuntimeFieldProperties = new FieldPropertiesCollection {
                            new FieldProperties(new RuntimeFieldIdentity("CustomerName"), new FieldSystemPropertyCollection {
                                new FieldSystemProperty("0B8889DC16EA4F32AFFED1A48E49A720", "First and Last Name"), // Value
                                new FieldSystemProperty("57491C9612194E639DBF85ECD794ECB2", 150)    // Width
                            }),
                            new FieldProperties(new RuntimeFieldIdentity("Address"), new FieldSystemPropertyCollection {
                                new FieldSystemProperty("0B8889DC16EA4F32AFFED1A48E49A720", "Street City Zip"), // Value
                                new FieldSystemProperty("57491C9612194E639DBF85ECD794ECB2", 250)    // Width
                            })
                        }
                    }, 0).DocumentId;
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdDocumentId;
        }

        /// <summary>
        /// It creates a document under the folder identified by parentId. It sets the document type and adds two pages.
        /// It returns the document Id of the new document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="parentId">Parent folder id.</param>
        /// <param name="filePath1">File path for page 1's image.</param>
        /// <param name="filePath2">File path for page 2's image.</param>
        /// <returns>The created document id.</returns>
        public string CreateDocumentWithPagesSample(string sessionId, string parentId, string filePath1, string filePath2)
        {
            string createdDocumentId = null;
            var cds = new sdk.CaptureDocumentService();
            var cts = new sdk.CategoryService();
            var ccs = new sdk.CaptureConfigurationService();

            try
            {
                // Retrieve categories
                var categoryFilter = new CategoryFilter()
                {
                    CategoryLevel = 2,
                    CheckAccess = false,
                    ParentCategory = new CategoryIdentity()
                };

                var categories = cts.GetCategories(sessionId, categoryFilter);

                // The document types we want are in a classification group that is under the "SDK Samples" category.
                var samplesCategory = categories.First(c => c.Identity.Name == "SDK Samples");

                // Get the classification groups that are under the samples category.
                var classificationGroupFilter = new ClassificationGroupFilter()
                {
                    Category = new CategoryIdentity()
                    {
                        Id = samplesCategory.Identity.Id,
                        Name = samplesCategory.Identity.Name,
                        ExtensionData = samplesCategory.Identity.ExtensionData
                    }
                };

                var classificationGroups = ccs.GetClassificationGroups(sessionId, classificationGroupFilter);

                // Now filter out the classification group that we want.
                var samplesClassificationGroup = classificationGroups.First(cg => cg.Identity.Name == "SDK Samples Group");

                // Finally, get all document types in the classification group.
                var docTypes = ccs.GetDocumentTypes(sessionId, samplesClassificationGroup.Identity);

                // Get the Northwest Order Forms document type identifier.
                var northwestTypeId = docTypes.First(d => d.Name == "NW Form").Identity.Id;

                // Create document.
                createdDocumentId = cds.CreateDocumentWithPages(sessionId, null, parentId, new RuntimeFieldCollection() {
                    new RuntimeField {
                        Name = "Valid",    // Document Valid property
                        Value = false
                    },
                    new RuntimeField {
                        Name = "ReviewValid",   // Document ReviewValid property
                        Value = false
                    }
                }, northwestTypeId, null, new PageDataCollection {
                    new PageData {
                        Data = File.ReadAllBytes(filePath1),
                        RuntimeFields = new RuntimeFieldCollection {
                            new RuntimeField {
                                Id = "9290D05F7D0F4F2786DA3DE0BBCBF891",    // Page Sheet Id
                                Value = "Sheet1"
                            },
                            new RuntimeField {
                                Id = "01FBA6D5EE40411FA435644376D8C316",    // Page Is Front
                                Value = true
                            }
                        }
                    },
                    new PageData {
                        Data = File.ReadAllBytes(filePath2),
                        RuntimeFields = new RuntimeFieldCollection {
                            new RuntimeField {
                                Id = "9290D05F7D0F4F2786DA3DE0BBCBF891",    // Page Sheet Id
                                Value = "Sheet1"
                            },
                            new RuntimeField {
                                Id = "01FBA6D5EE40411FA435644376D8C316",    // Page Is Front
                                Value = false
                            }
                        }
                    }
                }).DocumentId;
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdDocumentId;
        }

        /// <summary>
        /// It creates an online learning folder using 10 as the number of maximum document samples. It returns the
        /// Id of the folder.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <returns>The created folder's id.</returns>
        public string CreateOnlineLearningFolderSample(string sessionId)
        {
            string createdFolderId = null;
            var cds = new sdk.CaptureDocumentService();

            try
            {
                createdFolderId = cds.CreateOnlineLearningFolder(sessionId, 10);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdFolderId;
        }

        /// <summary>
        /// It copies the pages and properties from the document identified by docId to a newly created document.
        /// It returns the document Id of the new document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The document id.</param>
        /// <returns>The created document's id.</returns>
        public string CopyDocumentWithPages(string sessionId, string docId)
        {
            string createdFolderId = null;
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // An insertIndex of -1 will append the document to the end of the document list in the folder.
                createdFolderId = cds.CopyDocumentWithPages(sessionId, docId, -1);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return createdFolderId;
        }

        /// <summary>
        /// This method copies a source document properties, which includes the document’s type and data from
        /// only one field. Then it calls CopyDocument again to copy all properties and fields from the source document
        /// to the newly created document using its id as the target document id. It does not copy pages.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="sourceDocId">The source document id.</param>
        /// <param name="fieldNameToCopy">The name of the field to copy to the new document.</param>
        /// <returns>New document's id.</returns>
        public string CopyDocumentSample(string sessionId, string sourceDocId, string fieldNameToCopy)
        {
            var cds = new sdk.CaptureDocumentService();
            string newDocId;

            try
            {
                // Copy the source document's properties and data from just one of its fields to a new document.
                newDocId = cds.CopyDocument(sessionId, sourceDocId, null, new StringCollection() {
                    fieldNameToCopy
                }, 2);

                // Copy all of the source document's properties and field data to the newly created document.
                cds.CopyDocument(sessionId, sourceDocId, newDocId, new StringCollection(), 2);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return newDocId;
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// This method deletes the specified folder.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The folder id.</param>
        public void DeleteFolderSample(string sessionId, string folderId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Delete the folder
                cds.DeleteFolder(sessionId, folderId, null, false);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method deletes the specified document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The document id.</param>
        public void DeleteDocumentSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Delete the document
                cds.DeleteDocument(sessionId, docId, null, false);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method deletes pages from the specified document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The document id.</param>
        /// <param name="pageIndexes">The page indexes of the pages to be deleted.</param>
        public void DeletePagesSample(string sessionId, string docId, List<int> pageIndexes)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Delete the pages of the document.
                var pages = new PageIndexCollection();
                pages.AddRange(pageIndexes.Select(p => new PageIndex() { Index = p }));
                cds.DeletePages(sessionId, null, docId, pages);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method deletes the specified documents.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docIds">List of document ids to be deleted.</param>
        public void DeleteDocumentsSample(string sessionId, List<string> docIds)
        {
            var cds = new sdk.CaptureDocumentService();
            
            try
            {
                // Delete documents
                var documentIds = new StringCollection();
                documentIds.AddRange(docIds.AsEnumerable());
                cds.DeleteDocuments(sessionId, documentIds, null, false);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method deletes the specified extension of a document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The document id.</param>
        /// <param name="name">Name of the document extension.</param>
        public void DeleteExtensionSample(string sessionId, string docId, string name)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Delete the document
                cds.DeleteExtension(sessionId, null, docId, name);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }
        #endregion

        #region Update Methods

        /// <summary>
        /// It copies document properties and field values and properties from the source document to the
        /// destination document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="sourceDocId">The Id of a document with fields to be copied.</param>
        /// <param name="destDocId">The Id of a document with the same document type as the source document.</param>
        public void CopyDocumentFieldValuesSample (string sessionId, string sourceDocId, string destDocId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                cds.CopyDocumentFieldValues(sessionId, sourceDocId, destDocId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method inserts a new row into a table field of the specified document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The document id.</param>
        /// <param name="tableFieldIdOrName">The id or name of the document's table field.</param>
        public void InsertTableFieldRowSample(string sessionId, string docId, string tableFieldIdOrName)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                cds.InsertTableFieldRow(sessionId, null, docId, tableFieldIdOrName, -1);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }


        /// <summary>
        /// This method unlocks the given folder and/or document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The folder id.</param>
        /// <param name="docId">The document id.</param>
        public void ForceUnlockItemSample(string sessionId, string folderId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Unlock the document.
                cds.ForceUnlockItem(sessionId, null, new LockedItemIdentity() {
                    ItemId = docId,
                    ItemType = 0
                });

                // Unlock the folder.
                cds.ForceUnlockItem(sessionId, null, new LockedItemIdentity() {
                    ItemId = folderId,
                    ItemType = 1
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method saves a byte array representing an image to the database and returns the image id.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="batchId">A string that uniquely identifies the capture batch. This can be obtained in the context of an open batch through a form action.</param>
        /// <param name="filePath">The image file path.</param>
        /// <param name="mimeType">The image mime type.</param>
        /// <returns>Information about the saved image. <see cref="PageImageData"/></returns>
        public PageImageData SavePageImageSample(string sessionId, string batchId, string filePath, string mimeType)
        {
            var cds = new sdk.CaptureDocumentService();
            PageImageData pageImageData = null;

            try
            {
                var bytes = File.ReadAllBytes(filePath);
                pageImageData = cds.SavePageImage(sessionId, String.Empty, bytes, mimeType);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return pageImageData;
        }

        /// <summary>
        /// This method moves a folder to the first position under the root folder. Then it moves this
        /// folder into its sibling folder. Finally, it moves the folder to the original position.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be moved.</param>
        /// <param name="parentFolderId">The parent folder id.</param>
        /// <param name="siblingFolderId">The sibling folder id.</param>
        public void MoveFolderSample(string sessionId, string folderId, string parentFolderId, string siblingFolderId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var parentFolder = cds.GetFolder(sessionId, null, parentFolderId);
                var originalIndex = parentFolder.Folders.Select(f => f.Id).ToList().IndexOf(folderId);

                // Move folder to first position.
                cds.MoveFolder(sessionId, null, folderId, parentFolderId, 0);

                try
                {
                    // Move folder into sibling folder. Should throw exception.
                    // Folders must stay on the same folder level when they are moved.
                    cds.MoveFolder(sessionId, null, folderId, siblingFolderId, 0);
                }
                catch
                {

                }

                // Move folder to original position.
                cds.MoveFolder(sessionId, null, folderId, parentFolderId, originalIndex);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method moves a document to the specified folder and then it moves it back to the original folder.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be moved.</param>
        /// <param name="folderId">The folder id.</param>
        public void MoveDocumentSample(string sessionId, string docId, string folderId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var doc = cds.GetDocument(sessionId, null, docId);
                var parentFolder = cds.GetFolder(sessionId, null, doc.ParentId);
                var originalIndex = parentFolder.Documents.Select(d => d.Id).ToList().IndexOf(docId);

                // Move document to the given folder.
                cds.MoveDocument(sessionId, null, docId, folderId, 0);

                // Move document to original position.
                cds.MoveDocument(sessionId, null, docId, doc.ParentId, originalIndex);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method moves a page from a document to another document. Then it reorders pages within the
        /// second document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId1">The id of the document with a page to be moved.</param>
        /// <param name="pageIndexes">The indexes of the pages to be moved.</param>
        /// <param name="docId2">The id of the document receiving pages.</param>
        public void MovePagesSample(string sessionId, string docId1, List<int> pageIndexes, string docId2)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var pages = new PageIndexCollection();
                pages.AddRange(pageIndexes.Select(p => new PageIndex() { Index = p }));

                // Move the pages to the second document.
                cds.MovePages(sessionId, null, docId1, docId2, pages, 0);

                // Move the pages within the second document to the end.
                var doc2 = cds.GetDocument(sessionId, null, docId2);

                // Get the indexes of the pages' new location.
                var pages2 = new PageIndexCollection();

                for (int i = 0; i < pages.Count; i++)
                {
                    pages2.Add(new PageIndex() { Index = i });
                }

                // Move them to the end of the page list.
                cds.MovePages(sessionId, null, docId2, docId2, pages2, doc2.Pages.Count - 1);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method rejects the specified document with a reason.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be rejected.</param>
        /// <param name="reason">The reason for rejecting the document.</param>
        public void RejectDocumentSample(string sessionId, string docId, string reason)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                cds.RejectDocument(sessionId, docId, reason);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method un-rejects the specified documents (the documents' RejectionReason properties automatically become null).
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId1">The id of a document to be un-rejected.</param>
        /// <param name="docId2">The id of a document to be un-rejected.</param>
        public void UnrejectDocumentsSample(string sessionId, string docId1, string docId2)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                cds.UnrejectDocuments(sessionId, new StringCollection() {
                    docId1,
                    docId2
                }, null);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method rejects the specified pages with the same reason.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document with pages to be rejected.</param>
        /// <param name="pageIndexes">The indexes of the pages to be moved.</param>
        /// <param name="reason">The reason for rejecting the pages.</param>
        public void RejectPagesSample(string sessionId, string docId, List<int> pageIndexes, string reason)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var pages = new PageIndexCollection();
                pages.AddRange(pageIndexes.Select(p => new PageIndex() { Index = p }));
                cds.RejectPages(sessionId, docId, pages, reason);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method changes the state of the specified folder field.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder with a field to be updated.</param>
        /// <param name="fieldIdOrName">The id or name of the document's text field.</param>
        public void SetFolderFieldStatusSample(string sessionId, string folderId, string fieldIdOrName)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var fieldIdentity = new RuntimeFieldIdentity() { Name = fieldIdOrName, TableRow = -1, TableColumn = -1 };

                // Put field in Invalid state
                cds.SetFolderFieldStatus(sessionId, null, folderId, fieldIdentity, 0, "A sample error message.", null);

                // Put field in Valid state
                cds.SetFolderFieldStatus(sessionId, null, folderId, fieldIdentity, 1, null, null);

                // Put field in Force Valid state
                cds.SetFolderFieldStatus(sessionId, null, folderId, fieldIdentity, 2, null, null);

                // Put field in Confirmed state
                cds.SetFolderFieldStatus(sessionId, null, folderId, fieldIdentity, 3, null, "A sample value");

                try
                {
                    // Put field in Verified state. (Folder should be in a valid state prior to calling this.)
                    cds.SetFolderFieldStatus(sessionId, null, folderId, fieldIdentity, 4, null, null);
                }
                catch
                {
                    // Because the folder is not valid, the field cannot be verified.
                }

                // Put field in Unverified state
                cds.SetFolderFieldStatus(sessionId, null, folderId, fieldIdentity, 5, null, null);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method changes the state of the specified folder.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be updated.</param>
        public void SetFolderStatusSample(string sessionId, string folderId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Set folder to the "Review Invalid" state
                cds.SetFolderStatus(sessionId, null, folderId, 0, "A sample error message.");

                // Set folder to the "Review Valid" state
                cds.SetFolderStatus(sessionId, null, folderId, 1, null);

                // Override the folder's state
                cds.SetFolderStatus(sessionId, null, folderId, 2, null);

                // Restore the folder's state
                cds.SetFolderStatus(sessionId, null, folderId, 3, null);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method changes the state of the specified document field.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document with a field to be updated.</param>
        /// <param name="fieldIdOrName">The id or name of the document's text field.</param>
        public void SetDocumentFieldStatusSample(string sessionId, string docId, string fieldIdOrName)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var fieldIdentity = new RuntimeFieldIdentity() { Name = fieldIdOrName, TableRow = -1, TableColumn = -1 };

                // Put field in Invalid state
                cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 0, "A sample error message.", null);

                // Put field in Valid state
                cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 1, null, null); 

                // Put field in Force Valid state
                cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 2, null, null); 

                // Put field in Confirmed state
                cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 3, null, "A sample value"); 

                try
                {
                    // Put field in Verified state. (Document should be in a valid state prior to calling this.)
                    cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 4, null, null); 
                }
                catch
                {
                    // Because the document is not valid, the field cannot be verified.
                }

                // Put field in Unverified state
                cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 5, null, null); 

                // Update field's ExtractionConfident flag to true
                cds.SetDocumentFieldStatus(sessionId, null, docId, fieldIdentity, 6, null, true);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method changes the state of the specified document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        public void SetDocumentStatusSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Set document to the "Review Invalid" state
                cds.SetDocumentStatus(sessionId, null, docId, 0, "A sample error message.");

                // Set document to the "Review Valid" state
                cds.SetDocumentStatus(sessionId, null, docId, 1, null);

                // Override the document's state
                cds.SetDocumentStatus(sessionId, null, docId, 2, null);

                // Restore the document's state
                cds.SetDocumentStatus(sessionId, null, docId, 3, null);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method saves a page rendition and sets it to the first page of the specified document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="filePath">The file path to an image.</param>
        public void SetPageSourceImageFromRenditionSample(string sessionId, string docId, string filePath)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                var doc = cds.GetDocument(sessionId, null, docId);
                var pageId = doc.Pages[0].Id;
                var bytes = File.ReadAllBytes(filePath);
                cds.SavePageRendition(sessionId, docId, pageId, 1, "image/tif", bytes);
                cds.SetPageSourceImageFromRendition(sessionId, docId, pageId, 1);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method splits a folder at the specified document index.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be split.</param>
        /// <param name="docIndex">The document index at which to split the folder.</param>
        public string SplitFolderSample(string sessionId, string folderId, int docIndex)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Split folder at docIndex
                return cds.SplitFolder(sessionId, null, folderId, docIndex);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method splits a document at the specified page index and then merges it back.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be split.</param>
        /// <param name="pageIndex">The page index at which to split the document.</param>
        public void SplitDocumentSample(string sessionId, string docId, int pageIndex)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Split document at pageIndex
                string newDocId = cds.SplitDocument(sessionId, null, docId, pageIndex);

                // Merge the new document back with the original document.
                cds.MergeDocuments(sessionId, new StringCollection() { docId, newDocId });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method splits a document at the specified page index and will initialize various attributes
        /// of the new document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be split.</param>
        /// <param name="pageIndex">The page index at which to split the document.</param>
        /// <returns>The created document's id.</returns>
        public string SplitDocumentAndClassifySample(string sessionId, string docId, int pageIndex)
        {
            var cds = new sdk.CaptureDocumentService();
            string newDocId = null;

            try
            {
                // Use the document type from the original document.
                var doc = cds.GetDocument(sessionId, null, docId);

                // Split document at pageIndex. Initialize document with the given values.
                var docCollection = cds.SplitDocumentAndClassify(sessionId, null, docId, new SplitDocumentInfoCollection() {
                    new SplitDocumentInfo() {
                        SplitIndex = pageIndex,
                        DocumentTypeIdentity = new DocumentTypeIdentity() {
                            Id = doc.DocumentType.Identity.Id,
                            Name = doc.DocumentType.Identity.Name,
                            Version = doc.DocumentType.Identity.Version
                        },
                        ClassificationConfident = false,
                        ConfidenceLevel = 0.45,
                        ReviewValid = false
                    }
                });

                newDocId = docCollection[0].DocumentId;
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return newDocId;
        }

        /// <summary>
        /// This method merges the two specified documents.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId1">The id of the document to receive pages from the second document.</param>
        /// <param name="docId2">The id of the document to be merged into the first document.</param>
        public void MergeDocumentSample(string sessionId, string docId1, string docId2)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Merge the document identified by docId2 into the document identified by docId1.
                cds.MergeDocuments(sessionId, new StringCollection() { docId1, docId2 });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method updates a system field and a data field of the specified document in two
        /// separate API method calls of UpdateDocumentFieldValue.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="systemFieldIdOrName">The id or name of the document system field to be updated.</param>
        /// <param name="systemFieldValue">The value for the document system field.</param>
        /// <param name="dataFieldIdOrName">The id or name of the data field to be updated.</param>
        /// <param name="dataFieldValue">The value for the data field.</param>
        public void UpdateDocumentFieldValueSample(string sessionId, string docId, string systemFieldIdOrName, string systemFieldValue, string dataFieldIdOrName, string dataFieldValue)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Update the given system field.
                cds.UpdateDocumentFieldValue(sessionId, null, docId, new RuntimeField() {
                    Name = systemFieldIdOrName,
                    Value = systemFieldValue
                });

                // Update the given data field.
                cds.UpdateDocumentFieldValue(sessionId, null, docId, new RuntimeField() {
                    Name = dataFieldIdOrName,
                    Value = dataFieldValue
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining 
                // nodes on the process map.
                throw;
            }
        }


        public class LineItemDetail
        {
            public int Quantity { get; set; }
            public string Item { get; set; }
            public double Unit_Price { get; set; }
            public double Amount { get; set; }
        }


        /// <summary>
        /// This method updates a system field and a data field of the specified document in one
        /// API method call of UpdateDocumentFieldValues.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="systemFieldIdOrName">The id or name of the document system field to be updated.</param>
        /// <param name="systemFieldValue">The value for the document system field.</param>
        /// <param name="dataFieldIdOrName">The id or name of the data field to be updated.</param>
        /// <param name="dataFieldValue">The value for the data field.</param>
        public void UpdateDocumentFieldValuesSampleInsertLineitem(string sessionId, string docId, string tableID, int lineItemRowIndex, List<LineItemDetail> ltILineItemDetails)
        {
            
            var cds = new sdk.CaptureDocumentService();
            RuntimeFieldCollection runtimeFieldColl = new RuntimeFieldCollection();
            RuntimeField runtimeField = new RuntimeField();
            LineItemDetail ltDetail = new LineItemDetail();
            ltDetail = ltILineItemDetails[0];

            runtimeField = new RuntimeField();
            runtimeField.Id = tableID;
            runtimeField.TableRow = lineItemRowIndex;
            runtimeField.TableColumn = 1;
            runtimeField.Value = ltDetail.Quantity;
            runtimeField.Name = "Quantity"; 
            runtimeFieldColl.Add(runtimeField);

            runtimeField = new RuntimeField();
            runtimeField.Id = tableID;
            runtimeField.TableRow = lineItemRowIndex;
            runtimeField.TableColumn = 1;
            runtimeField.Value = ltDetail.Item;
            runtimeField.Name = "Item";
            runtimeFieldColl.Add(runtimeField);

            runtimeField = new RuntimeField();
            runtimeField.Id = tableID;
            runtimeField.TableRow = lineItemRowIndex;
            runtimeField.TableColumn = 1;
            runtimeField.Value = ltDetail.Unit_Price;
            runtimeField.Name = "Unit Price";
            runtimeFieldColl.Add(runtimeField);

            runtimeField = new RuntimeField();
            runtimeField.Id = tableID;
            runtimeField.TableRow = lineItemRowIndex;
            runtimeField.TableColumn = 1;
            runtimeField.Value = ltDetail.Amount;
            runtimeField.Name = "Amount";
            runtimeFieldColl.Add(runtimeField);

            try
            {
                // Update the given system field and data field of the document.
                cds.UpdateDocumentFieldValues(sessionId, null, docId, runtimeFieldColl);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method updates a document's type.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="docTypeId">The id of the document type.</param>
        public void UpdateDocumentTypeSample(string sessionId, string docId, string docTypeId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                cds.UpdateDocumentType(sessionId, docId, new DocumentTypeIdentity() {
                    Id = docTypeId
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method updates a document's type, the confidence for that document type, and whether the classfication is confident or not.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="docTypeId">The id of the document type.</param>
        /// <param name="confidenceLevel">The confidence level for the document type.</param>
        /// <param name="classificationConfident">Whether the classification is confident or not.</param>
        public void UpdateDocumentTypeWithConfidence2(string sessionId, string docId, string docTypeId, double confidenceLevel, bool classificationConfident)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                cds.UpdateDocumentTypeWithConfidence2(sessionId, docId, new DocumentTypeIdentity() {
                    Id = docTypeId
                }, confidenceLevel, classificationConfident);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method updates a system field and a data field of the specified folder in two
        /// separate API method calls of UpdateFolderFieldValue.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be updated.</param>
        /// <param name="systemFieldIdOrName">The id or name of the folder system field to be updated.</param>
        /// <param name="systemFieldValue">The value for the folder system field.</param>
        /// <param name="dataFieldIdOrName">The id or name of the data field to be updated.</param>
        /// <param name="dataFieldValue">The value for the data field.</param>
        public void UpdateFolderFieldValueSample(string sessionId, string folderId, string systemFieldIdOrName, string systemFieldValue, string dataFieldIdOrName, string dataFieldValue)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Update the given system field.
                if (!string.IsNullOrWhiteSpace(systemFieldIdOrName))
                {
                    cds.UpdateFolderFieldValue(sessionId, null, folderId, new RuntimeField() {
                        Name = systemFieldIdOrName,
                        Value = systemFieldValue
                    });
                }

                // Update the given data field.
                if (!string.IsNullOrWhiteSpace(dataFieldIdOrName))
                {
                    cds.UpdateFolderFieldValue(sessionId, null, folderId, new RuntimeField() {
                        Name = dataFieldIdOrName,
                        Value = dataFieldValue
                    });
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// This method updates a system field and a data field of the specified folder in one
        /// API method call of UpdateFolderFieldValues.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be updated.</param>
        /// <param name="systemFieldIdOrName">The id or name of the folder system field to be updated.</param>
        /// <param name="systemFieldValue">The value for the folder system field.</param>
        /// <param name="dataFieldIdOrName">The id or name of the data field to be updated.</param>
        /// <param name="dataFieldValue">The value for the data field.</param>
        public void UpdateFolderFieldValuesSample(string sessionId, string folderId, string systemFieldIdOrName, string systemFieldValue, string dataFieldIdOrName, string dataFieldValue)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Update the given system field and data field of the folder.
                if (!string.IsNullOrWhiteSpace(dataFieldIdOrName))
                {
                    cds.UpdateFolderFieldValues(sessionId, null, folderId, new RuntimeFieldCollection() {
                        new RuntimeField() {
                            Name = systemFieldIdOrName,
                            Value = systemFieldValue
                        },
                        new RuntimeField() {
                            Name = dataFieldIdOrName,
                            Value = dataFieldValue
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// Updates values and properties of three fields of the document identified by docId.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="field1">The id or name of a document field to be updated.</param>
        /// <param name="field1Value">The value to set for field1.</param>
        /// <param name="field1Error">The error message to set for field1.</param>
        /// <param name="field2">The id or name of a second document field to be updated.</param>
        /// <param name="field2Value">The value to set for field2.</param>
        /// <param name="field2ExtractionConfident">The ExtractionConfident value to set for field2.</param>
        /// <param name="field3">The id or name of a third document field to be updated.</param>
        /// <param name="field3Row">The row of the third document field to be updated.</param>
        /// <param name="field3Col">The column of the third document field to be updated.</param>
        /// <param name="field3Value">The value to set for field3.</param>
        /// <param name="field3Width">The Width value to set for field3.</param>
        public void UpdateDocumentFieldPropertyValuesSample(string sessionId, string docId, string field1,
            string field1Value, string field1Error, string field2, string field2Value, bool field2ExtractionConfident,
            string field3, int field3Row, int field3Col, string field3Value, string field3Width)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Update the given system field.
                cds.UpdateDocumentFieldPropertyValues(sessionId, null, docId, new FieldPropertiesCollection {
                    new FieldProperties {
                        Identity = new RuntimeFieldIdentity(field1),
                        PropertyCollection = new FieldSystemPropertyCollection {
                            new FieldSystemProperty {
                                SystemFieldIdentity = new FieldSystemPropertyIdentity {
                                    Name = "Value"
                                },
                                Value = field1Value
                            },
                            new FieldSystemProperty {
                                SystemFieldIdentity = new FieldSystemPropertyIdentity {
                                    Name = "ErrorDescription"
                                },
                                Value = field1Error
                            }
                        }
                    },
                    new FieldProperties {
                        Identity = new RuntimeFieldIdentity(field2),
                        PropertyCollection = new FieldSystemPropertyCollection {
                            new FieldSystemProperty {
                                SystemFieldIdentity = new FieldSystemPropertyIdentity {
                                    Name = "Value"
                                },
                                Value = field2Value
                            },
                            new FieldSystemProperty {
                                SystemFieldIdentity = new FieldSystemPropertyIdentity {
                                    Name = "ExtractionConfident"
                                },
                                Value = field2ExtractionConfident
                            }
                        }
                    },
                    new FieldProperties {
                        Identity = new RuntimeFieldIdentity(field3, field3Row, field3Col),
                        PropertyCollection = new FieldSystemPropertyCollection {
                            new FieldSystemProperty {
                                SystemFieldIdentity = new FieldSystemPropertyIdentity {
                                    Name = "Value"
                                },
                                Value = field3Value
                            },
                            new FieldSystemProperty {
                                SystemFieldIdentity = new FieldSystemPropertyIdentity {
                                    Name = "Width"
                                },
                                Value = field3Width
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// It updates the first page of the document identified by docId with the provided data.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="pageIndex">The index of the page to update.</param>
        /// <param name="sheetId">The sheetId to assign to the specified page.</param>
        /// <param name="width">The width to assign to the specified page.</param>
        /// <param name="barcodeWidth">The width to assign to a barcode to be added to the specified page.</param>
        /// <param name="barcodeHeight">The height to assign to a barcode to be added to the specified page.</param>
        public void UpdatePagesSample (string sessionId, string docId, int pageIndex, string sheetId, int width, int barcodeWidth, int barcodeHeight)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Update the specified page (index = pageIndex) with the given properties.
                cds.UpdatePages(sessionId, null, docId, new PageData2Collection {
                    new PageData2 {
                        Index = pageIndex,
                        UseSheetId = true,
                        SheetId = sheetId,
                        UseIsFront = true,
                        IsFront = true,
                        UseHorizontalResolution = false,
                        UseVerticalResolution = false,
                        UseRotationType = true,
                        RotationType = 0,
                        UseLayoutWidth = true,
                        LayoutWidth = width,
                        UseLayoutHeight = false,
                        UseImprintedText = false,
                        UseVrsProcessed = false,
                        UseBarcodes = true,
                        Barcodes = new BarcodeCollection {
                            new Barcode {
                                Width = barcodeWidth,
                                Height = barcodeHeight,
                                Value = "Sample Barcode"
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        /// <summary>
        /// It retrieves the document identified by docId and updates its source file with the file located at filePath.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be updated.</param>
        /// <param name="filePath">The path of the file to be retrieved and used by this method.</param>
        public void UpdateSourceFileSample (string sessionId, string docId, string filePath)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Update the source file with the given file.
                cds.UpdateSourceFile(sessionId, null, docId, new DocumentSourceFile {
                    SourceFile = File.ReadAllBytes(filePath)
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        #endregion

        #region Validate Methods

        /// <summary>
        /// This method validates a document by validating all of its fields. 
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be checked for validity.</param>
        /// <returns>A boolean value indicating if the document's fields are valid.</returns>
        public bool ValidateDocumentSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            bool isValid = false;

            try
            {
                isValid = cds.ValidateDocument(sessionId, docId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return isValid;
        }

        /// <summary>
        /// This method validates a document field based on the validation rules defined for the field in the
        /// document type.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docTypeId">The id of the document type.</param>
        /// <param name="fieldName">The name of a document field.</param>
        /// <param name="fieldValue">The value to use for the document field.</param>
        /// <returns>A result indicating if the document field is valid for the given type and value. <see cref="ValidationResult"/></returns>
        public ValidationResult ValidateDocumentFieldSample(string sessionId, string docTypeId, string fieldName, string fieldValue)
        {
            var cds = new sdk.CaptureDocumentService();
            ValidationResult validationResult = null;

            try
            {
                validationResult = cds.ValidateDocumentField(sessionId, docTypeId, new RuntimeFieldValue() {
                    Name = fieldName,
                    Value = fieldValue
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return validationResult;
        }

        /// <summary>
        /// It will validate the given fields against the corresponding field validators and formatters of the specified document type and concatenate and return the results.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docTypeId">The id of the document type.</param>
        /// <param name="field1">The name of a document field.</param>
        /// <param name="field1Value">The value to use for the document field.</param>
        /// <param name="field2">The name of a second document field.</param>
        /// <param name="field2Value">The value to use for the second document field.</param>
        /// <returns>The concatenated results of the validation of the specified fields.</returns>
        public string ValidateDocumentFieldsSample(string sessionId, string docTypeId, string field1, string field1Value, string field2, string field2Value)
        {
            var cds = new sdk.CaptureDocumentService();
            ValidationResultCollection validationResult = null;

            try
            {
                validationResult = cds.ValidateDocumentFields(sessionId, docTypeId, new RuntimeFieldValueCollection {
                    new RuntimeFieldValue {
                        Name = field1,
                        Value = field1Value
                    },
                    new RuntimeFieldValue {
                        Name = field2,
                        Value = field2Value
                    }
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return field1 + ": " + validationResult[0].IsValid + ", " + field2 + ": " + validationResult[1].IsValid;
        }

        /// <summary>
        /// It will validate the specified fields against the corresponding field validators and formatters of the specified document type and concatenate and return the results.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docTypeId">The id of the document type.</param>
        /// <param name="field1">The name of a document field.</param>
        /// <param name="field1Value">The value to use for the document field.</param>
        /// <param name="field2">The name of a second document field.</param>
        /// <param name="field2Value">The value to use for the second document field.</param>
        /// <returns>The concatenated results of the validation of the specified fields.</returns>
        public string ValidateAllDocumentFieldsSample (string sessionId, string docTypeId, string field1, string field1Value, string field2, string field2Value)
        {
            var cds = new sdk.CaptureDocumentService();
            FieldValidationResult2Collection validationResult = null;

            try
            {
                validationResult = cds.ValidateAllDocumentFields(sessionId, new DocumentTypeIdentity {
                    Id = docTypeId
                }, new RuntimeField2Collection {
                    new RuntimeField2 {
                        Name = field1,
                        Value = field1Value
                    },
                    new RuntimeField2 {
                        Name = field2,
                        Value = field2Value
                    }
                }, new DocumentSystemProperties {
                    NoOfPages = 2,
                    Rejected = false,
                    ReviewValid = true
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return field1 + ": " + validationResult[0].IsValid + ", " + field2 + ": " + validationResult[1].IsValid;
        }

        /// <summary>
        /// It will validate the specified field of the given document and return the result for it.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>A result indicating if the specified document field is valid. <see cref="FieldValidationResult2"/></returns>
        public FieldValidationResult2 RunDocumentFieldsValidationSample (string sessionId, string docId, string fieldName)
        {
            var cds = new sdk.CaptureDocumentService();
            FieldValidationResult2Collection validationResult = null;

            try
            {
                validationResult = cds.RunDocumentFieldsValidation(sessionId, docId, new RuntimeFieldIdentityCollection {
                    new RuntimeFieldIdentity {
                        Name = fieldName
                    }
                });
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return validationResult[0];
        }

        /// <summary>
        /// This method validates the specified document against the document's validation rules.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be checked for validity.</param>
        /// <returns>A result indicating if the document is reviewed as valid. <see cref="ReviewValidationResult"/></returns>
        public ReviewValidationResult ValidateDocumentForReviewSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            ReviewValidationResult reviewValidationResult = null;

            try
            {
                reviewValidationResult = cds.ValidateDocumentForReview(sessionId, docId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return reviewValidationResult;
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// This method retrieves the specified document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be retrieved.</param>
        /// <returns>Returns a Document object, containing a collection of pages, document Mime type and various other attributes such as whether it was rejected or not, the reason for rejection etc. <see cref="Document"/></returns>
        public Document GetDocumentSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            Document doc = null;

            try
            {
                doc = cds.GetDocument(sessionId, null, docId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return doc;
        }

        /// <summary>
        /// This method retrieves the source file of the specified document. It returns the length of the returned file stream.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be retrieved.</param>
        /// <param name="fileType">The file type to be retrieved.</param>
        /// <returns>The length of the returned file stream.</returns>
        public long GetDocumentFileSample(string sessionId, string docId, string fileType)
        {
            var cds = new sdk.CaptureDocumentService();
            long streamLength = 0;

            try
            {
                using (var fileStream = cds.GetDocumentFile(sessionId, null, docId, fileType))
                {
                    if (fileStream != null)
                    {
                        streamLength = fileStream.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return streamLength;
        }

        /// <summary>
        /// This method retrieves the field alternatives of a field from the specified document and returns the first field alternative's Text property.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be retrieved.</param>
        /// <param name="fieldIdOrName">The id or name of the field.</param>
        /// <returns>It returns the first field alternative's Text property, otherwise null if not found.</returns>
        public string GetDocumentFieldAlternatives2Sample(string sessionId, string docId, string fieldIdOrName)
        {
            var cds = new sdk.CaptureDocumentService();
            string alt = null;

            try
            {
                var fieldAlts = cds.GetDocumentFieldsAlternatives2(sessionId, docId, new FieldIdentityCollection()
                {
                    new FieldIdentity(fieldIdOrName)
                }, 1);

                if (fieldAlts.Count > 0)
                {
                    var items = fieldAlts[0].DocumentFieldAlternatives2;

                    if (items.Count > 0)
                    {
                        alt = items[0].Text;
                    }
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return alt;
        }

        /// <summary>
        /// This method retrieves the value for the specified document field, including pre-defined document system fields such as verified, rejected, number of pages etc.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document to be retrieved.</param>
        /// <param name="systemFieldIdOrName">The id or name of the document system field to be retrieved.</param>
        /// <param name="dataFieldIdOrName">The id or name of the data field to be retrieved.</param>
        /// <returns>Returns both properties in one string result as [system field value]|[data field value].</returns>
        public string GetDocumentFieldValueSample(string sessionId, string docId, string systemFieldIdOrName, string dataFieldIdOrName)
        {
            var cds = new sdk.CaptureDocumentService();
            string values = null;

            try
            {
                // Retrieve the given system field of the document.
                var docValue = cds.GetDocumentFieldValue(sessionId, null, docId,
                    new RuntimeFieldIdentity() {
                        Name = systemFieldIdOrName
                    });

                if (docValue != null)
                {
                    values += (docValue.Value ?? String.Empty) + "|";
                }

                // Retrieve the given data field of the document.
                docValue = cds.GetDocumentFieldValue(sessionId, null, docId,
                    new RuntimeFieldIdentity() {
                        Name = dataFieldIdOrName
                    });

                if (docValue != null)
                {
                    values += docValue.Value ?? String.Empty;
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return values;
        }

        /// <summary>
        /// This method retrieves the specified folder.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be retrieved.</param>
        /// <returns>Returns a Folder object, containing a collection of documents, folders, and various other attributes such as its parent folder. <see cref="Folder"/></returns>
        public Folder GetFolderSample(string sessionId, string folderId)
        {
            var cds = new sdk.CaptureDocumentService();
            Folder folder = null;

            try
            {
                folder = cds.GetFolder(sessionId, null, folderId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return folder;
        }

        /// <summary>
        /// This method retrieves a system field and a data field of the specified folder in one
        /// API method call of UpdateFolderFieldValues.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="folderId">The id of the folder to be updated.</param>
        /// <param name="systemFieldIdOrName">The id or name of the folder system field to be retrieved.</param>
        /// <param name="dataFieldIdOrName">The id or name of the data field to be retrieved.</param>
        /// <returns>Returns both properties in one string result as [system field value]|[data field value].</returns>
        public string GetFolderFieldValuesSample(string sessionId, string folderId, string systemFieldIdOrName, string dataFieldIdOrName)
        {
            var cds = new sdk.CaptureDocumentService();
            string values = null;

            try
            {
                // Retrieve the given system field and data field of the folder.
                if (!string.IsNullOrWhiteSpace(systemFieldIdOrName) && !string.IsNullOrWhiteSpace(dataFieldIdOrName))
                {
                    var folderValues = cds.GetFolderFieldValues(sessionId, null, folderId, new RuntimeFieldIdentityCollection() {
                        new RuntimeFieldIdentity() {
                            Name = systemFieldIdOrName
                        },
                        new RuntimeFieldIdentity() {
                            Name = dataFieldIdOrName
                        }
                    });

                    for (int i = 0; i < folderValues.Count; i++)
                    {
                        values += (folderValues[i].Value ?? String.Empty) + (i == 0 ? "|" : String.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return values;
        }

        /// <summary>
        /// This method retrieves the image of the first page of the specified document, and returns the length of the corresponding image byte array.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document with a page.</param>
        /// <returns>Returns the length of the byte array representation of the image.</returns>
        public int GetImageSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            int imageArrayLength = -1;

            try
            {
                // Retrieve the document.
                var doc = cds.GetDocument(sessionId, null, docId);

                if (doc.Pages.Count > 0)
                {
                    // Retrieve the image.
                    var image = cds.GetImage(sessionId, null, doc.Pages[0].ImageId, -1, -1, "tif");
                    imageArrayLength = image.Image.Length;
                }
                else
                {
                    throw new Exception(string.Format("The document with an id of {0} does not contain any pages.", docId));
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return imageArrayLength;
        }

        /// <summary>
        /// This method retrieves page rendition 1 from the first page of the specified document, and returns the length of the corresponding image byte array.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document with a page rendition corresponding to its first page.</param>
        /// <returns>Returns the length of the byte array representation of page rendition 1 for the first page.</returns>
        public int GetPageRenditionSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            int renditionArrayLength = -1;

            try
            {
                // Retrieve the document.
                var doc = cds.GetDocument(sessionId, null, docId);

                if (doc.Pages.Count > 0)
                {
                    // Retrieve the page rendition.
                    var pageId = doc.Pages[0].Id;
                    var pageRendition = cds.GetPageRendition(sessionId, docId, pageId, 1);
                    renditionArrayLength = pageRendition.Length;
                }
                else
                {
                    throw new Exception(string.Format("The document with an id of {0} does not contain any pages.", docId));
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return renditionArrayLength;
        }

        /// <summary>
        /// This method saves a page text extension using SavePageTextExtension to the first page of the specified document, and then retrieves it using GetPageTextExtension.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document with at least one page.</param>
        /// <param name="textExtensionName">The name of the text extension.</param>
        /// <returns>Returns the saved page text extension.</returns>
        public string GetPageTextExtensionSample(string sessionId, string docId, string textExtensionName)
        {
            var cds = new sdk.CaptureDocumentService();
            string savedTextExtension = null;

            try
            {
                // Retrieve the document.
                var doc = cds.GetDocument(sessionId, null, docId);

                if (doc.Pages.Count > 0)
                {
                    // Save the text extension.
                    var pageId = doc.Pages[0].Id;
                    cds.SavePageTextExtension(sessionId, null, docId, pageId, textExtensionName, "A value to store.");

                    // Retrieve the text extension.
                    savedTextExtension = cds.GetPageTextExtension(sessionId, null, docId, pageId, textExtensionName);
                }
                else
                {
                    throw new Exception(string.Format("The document with an id of {0} does not contain any pages.", docId));
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return savedTextExtension;
        }

        /// <summary>
        /// This method retrieves the validation execution context which determines the component and place within the job/process where validation logic is executing.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <returns>Returns validation execution context object. <see cref="ValidationExecutionContext"/></returns>
        public ValidationExecutionContext GetValidationExecutionContextSample(string sessionId)
        {
            var cds = new sdk.CaptureDocumentService();
            ValidationExecutionContext validationExecutionContext = null;

            try
            {
                validationExecutionContext = cds.GetValidationExecutionContext(sessionId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return validationExecutionContext;
        }

        /// <summary>
        /// This method retrieves rejected pages within a document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document with the rejected pages.</param>
        /// <returns>Returns <see cref="RejectedPages"/> object, which has a attribute which indicates if there are rejected pages (HasRejectedPage). There is also a collection page indexes of the rejected pages</returns>
        public RejectedPages GetRejectedPagesSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            RejectedPages rejectedPages = null;

            try
            {
                rejectedPages = cds.GetRejectedPages(sessionId, docId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return rejectedPages;
        }

        /// <summary>
        /// This method retrieves the source file of a document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document.</param>
        /// <returns>Returns <see cref="DocumentSourceFile"/> object, which contains the source file as a byte array and mime type.</returns>
        public DocumentSourceFile GetSourceFileSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();
            DocumentSourceFile documentSourceFile = null;

            try
            {
                documentSourceFile = cds.GetSourceFile(sessionId, null, docId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return documentSourceFile;
        }

        /// <summary>
        /// This method retrieves a text extension of a document.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document.</param>
        /// <param name="name">The name of the text extension to retrieve. Cannot be null or empty</param>
        /// <returns>The text stored in the document with the specified name.</returns>
        /// @par Remarks
        /// Text extensions are name/value pairs that allow custom string data to be stored and retrieved by a specified extension name.
        /// @par Security
        public string GetTextExtensionSample(string sessionId, string docId, string name)
        {
            var cds = new sdk.CaptureDocumentService();
            string textExtension = null;

            try
            {
                textExtension = cds.GetTextExtension(sessionId, null, docId, name);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return textExtension;
        }

        /// <summary>
        /// This method retrieves summary of page image.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document.</param>
        /// <param name="pageId">The id of the page.</param>
        /// <returns>Returns <see cref="PageSummary"/> object, which contains information of the image.</returns>
        public PageSummary GetPageSummarySample(string sessionId, string docId, string pageId)
        {
            var cds = new sdk.CaptureDocumentService();
            PageSummary pageSummary = null;

            try
            {
                pageSummary = cds.GetPageSummary(sessionId, docId, pageId);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return pageSummary;
        }

        /// <summary>
        /// This method retrieves summary of page rendition image.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document.</param>
        /// <param name="pageId">The id of the page.</param>
        /// <returns>Returns <see cref="ImageSummary"/> object, which contains information of the rendition image.</returns>
        public ImageSummary GetPageRenditionImageSummarySample(string sessionId, string docId, string pageId, short renditionNumber)
        {
            var cds = new sdk.CaptureDocumentService();
            ImageSummary imageSummary = null;

            try
            {
                imageSummary = cds.GetPageRenditionImageSummary(sessionId, docId, pageId, renditionNumber);
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }

            return imageSummary;
        }

        /// <summary>
        /// This method retrieves system properties of a page.
        /// </summary>
        /// <param name="sessionId">A string that uniquely identifies the session for the current logged on user. If the sessionId is invalid, then an exception will be raised.</param>
        /// <param name="docId">The id of the document.</param>
        public void GetPagePropertyValuesSample(string sessionId, string docId)
        {
            var cds = new sdk.CaptureDocumentService();

            try
            {
                // Get document to get pages
                Document doc = cds.GetDocument(sessionId, null, docId);

                PagePropertiesIdentityCollection pagePropertiesIdentities = new PagePropertiesIdentityCollection();
                
                foreach (Page page in doc.Pages)
                {
                    // Get all field property identities for the page
                    FieldSystemPropertyIdentityCollection fieldPropertyIdentities = new FieldSystemPropertyIdentityCollection();
                    
                    // Using Name attribute for this example, but including the GUID for the property if using Id attribute
                    // Id="4F8BF8ABF72844AFB601D79F01FCD172", return type is CaptureAnnotationCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "Annotations" });
                    // Id="C7091EE3F1764D9B830A2DCCDDE3BC66", return type is BarcodeCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "Barcodes" });
                    // Id="392BB1F05CA64276BA89B3D7F69C11A7", return type is ClassificationResultCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "ContentClassificationResults" });
                    // Id="01FBA6D5EE40411FA435644376D8C316", return type is Boolean
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "IsFront" });
                    // Id="05D0FE805F6A4D53A7538816084C2A9E", return type is Float
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "HorizontalResolution" });
                    // Id="F938266C4CC640FC8C289D1FE732CD3E", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "InstanceId" });
                    // Id="57958466808145E0BD47603B0606B0E4", return type is Int32
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "PageIndex" });
                    // Id="D576DDE6FE3649CEB32DFD28845487C3", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "ImageId" });
                    // Id="C669842EA20A47F0A0F6D48EB0F3FBEE", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "ImprintedText" });
                    // Id="0E28C018E3294BE0A89789078EB0DE23", return type is ClassificationResultCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "LayoutClassificationResults" });
                    // Id="EDDA201B9E9C46BD8121F24B2E72A1CE", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "MimeType" });
                    // Id="CD0F9328210B4DE4B9B8A306B20BF8FC", return type is Boolean
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "IsRejected" });
                    // Id="56840E9FF63F47BD83EF726E9B989A0A", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "RejectionNote" });
                    // Id="A1EA890B8FF042BC8FD7970F0DFD2E6F", return type is Boolean
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "RightToLeft" });
                    // Id="D4E9F5534C484840A56CC20F0730AA92", return type is Enum
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "RotationType" });
                    // Id="9290D05F7D0F4F2786DA3DE0BBCBF891", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "SheetId" });
                    // Id="0D5434EA13E643D19E84881202A74271", return type is Int32
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "Height" });
                    // Id="57491C9612194E639DBF85ECD794ECB2", return type is Int32
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "Width" });
                    // Id="E60063B1FD624F9D989023916E21BEE8", return type is Byte[]
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "SourceFileData" });
                    // Id="1187BF2F5A2E4ADEB6ACD483296ED2DE", return type is Boolean
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "SplitPage" });
                    // Id="0EBAEBB3DC3F468694D2A49E623D63C2", return type is ClassificationResultCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "TdsResults" });
                    // Id="7816A234B4234181B93F9E134B79C6EE", return type is TextLineCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "TextLines" });
                    // Id="260A3F338640445FB35F41A778C54518", return type is String
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "ThumbnailId" });
                    // Id="D101F18AF1FC4E848E0F08B8384066DC", return type is Float
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "VerticalResolution" });
                    // Id="9F36442A45814F48A5A140D05D7072C4", return type is Boolean
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "VrsProcessed" });
                    // Id="D81F2877289147E4915D9168F2E53020", return type is WordCollection
                    fieldPropertyIdentities.Add(new FieldSystemPropertyIdentity() { Name = "Words" });

                    // Create page property identities for each page in the document and apply properties for each page
                    pagePropertiesIdentities.Add(new PagePropertiesIdentity(page.Id, fieldPropertyIdentities));
                }

                PagePropertiesCollection pageProperties = cds.GetPagePropertyValues(sessionId, docId, pagePropertiesIdentities);

                // Retrieve property values and convert to their defined data types
                foreach (PageProperties pageProperty in pageProperties)
                {
                    string pageId = pageProperty.Id;
                    ExtensionDataObject extensionData = pageProperty.ExtensionData;
                    foreach (FieldSystemProperty fieldSystemProperty in pageProperty.PropertyCollection)
                    {
                        FieldSystemPropertyIdentity fieldSystemPropertyIdentity = fieldSystemProperty.SystemFieldIdentity;

                        if (fieldSystemPropertyIdentity.Id == "4F8BF8ABF72844AFB601D79F01FCD172" &&
                            fieldSystemPropertyIdentity.Name == "Annotations")
                        {
                            CaptureAnnotationCollection annotations = (CaptureAnnotationCollection) fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "C7091EE3F1764D9B830A2DCCDDE3BC66" &&
                                 fieldSystemPropertyIdentity.Name == "Barcodes")
                        {
                            BarcodeCollection barcodes = (BarcodeCollection)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "392BB1F05CA64276BA89B3D7F69C11A7" &&
                                 fieldSystemPropertyIdentity.Name == "ContentClassificationResults")
                        {
                            ClassificationResultCollection contentClassificationResults = (ClassificationResultCollection)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "01FBA6D5EE40411FA435644376D8C316" &&
                                 fieldSystemPropertyIdentity.Name == "IsFront")
                        {
                            Boolean isFront = (Boolean)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "05D0FE805F6A4D53A7538816084C2A9E" &&
                                 fieldSystemPropertyIdentity.Name == "HorizontalResolution")
                        {
                            float horizontalResolution = (float)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "F938266C4CC640FC8C289D1FE732CD3E" &&
                                 fieldSystemPropertyIdentity.Name == "InstanceId")
                        {
                            String instanceId = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "57958466808145E0BD47603B0606B0E4" &&
                                 fieldSystemPropertyIdentity.Name == "PageIndex")
                        {
                            Int32 pageIndex = (Int32)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "D576DDE6FE3649CEB32DFD28845487C3" &&
                                 fieldSystemPropertyIdentity.Name == "ImageId")
                        {
                            String imageId = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "C669842EA20A47F0A0F6D48EB0F3FBEE" &&
                                 fieldSystemPropertyIdentity.Name == "ImprintedText")
                        {
                            String imprintedText = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "0E28C018E3294BE0A89789078EB0DE23" &&
                                 fieldSystemPropertyIdentity.Name == "LayoutClassificationResults")
                        {
                            ClassificationResultCollection layoutClassificationResults = (ClassificationResultCollection)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "EDDA201B9E9C46BD8121F24B2E72A1CE" &&
                                 fieldSystemPropertyIdentity.Name == "MimeType")
                        {
                            String mimeType = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "CD0F9328210B4DE4B9B8A306B20BF8FC" &&
                                 fieldSystemPropertyIdentity.Name == "IsRejected")
                        {
                            Boolean isRejected = (Boolean)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "56840E9FF63F47BD83EF726E9B989A0A" &&
                                 fieldSystemPropertyIdentity.Name == "RejectionNote")
                        {
                            String rejectionNote = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "A1EA890B8FF042BC8FD7970F0DFD2E6F" &&
                                 fieldSystemPropertyIdentity.Name == "RightToLeft")
                        {
                            Boolean rightToLeft = (Boolean)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "D4E9F5534C484840A56CC20F0730AA92" &&
                                 fieldSystemPropertyIdentity.Name == "RotationType")
                        {
                            Enum rotationType = (Enum)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "9290D05F7D0F4F2786DA3DE0BBCBF891" &&
                                 fieldSystemPropertyIdentity.Name == "SheetId")
                        {
                            String sheetId = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "0D5434EA13E643D19E84881202A74271" &&
                                 fieldSystemPropertyIdentity.Name == "Height")
                        {
                            Int32 height = (Int32)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "57491C9612194E639DBF85ECD794ECB2" &&
                                 fieldSystemPropertyIdentity.Name == "Width")
                        {
                            Int32 width = (Int32)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "E60063B1FD624F9D989023916E21BEE8" &&
                                 fieldSystemPropertyIdentity.Name == "SourceFileData")
                        {
                            Byte[] sourceFileData = (Byte[])fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "1187BF2F5A2E4ADEB6ACD483296ED2DE" &&
                                 fieldSystemPropertyIdentity.Name == "SplitPage")
                        {
                            Boolean splitPage = (Boolean)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "0EBAEBB3DC3F468694D2A49E623D63C2" &&
                                 fieldSystemPropertyIdentity.Name == "TdsResults")
                        {
                            ClassificationResultCollection tdsResults = (ClassificationResultCollection)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "7816A234B4234181B93F9E134B79C6EE" &&
                                 fieldSystemPropertyIdentity.Name == "TextLines")
                        {
                            TextLineCollection textLines = (TextLineCollection)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "260A3F338640445FB35F41A778C54518" &&
                                 fieldSystemPropertyIdentity.Name == "ThumbnailId")
                        {
                            String thumbnailId = (String)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "D101F18AF1FC4E848E0F08B8384066DC" &&
                                 fieldSystemPropertyIdentity.Name == "VerticalResolution")
                        {
                            float verticalResolution = (float)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "9F36442A45814F48A5A140D05D7072C4" &&
                                 fieldSystemPropertyIdentity.Name == "VrsProcessed")
                        {
                            Boolean vrsProcessed = (Boolean)fieldSystemProperty.Value;
                        }
                        else if (fieldSystemPropertyIdentity.Id == "D81F2877289147E4915D9168F2E53020" &&
                                 fieldSystemPropertyIdentity.Name == "Words")
                        {
                            WordCollection words = (WordCollection)fieldSystemProperty.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Unhandled exceptions will terminate processing from progressing through the remaining
                // nodes on the process map.
                throw;
            }
        }

        #endregion
    }
}
