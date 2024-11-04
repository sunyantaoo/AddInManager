using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Reflection;
using Autodesk.AutoCAD.Geometry;
using System;

[assembly: CommandClass(typeof(Sample001.Class1))]
namespace Sample001
{
    /// <summary>
    /// 生成命令
    /// </summary>
    [Serializable]
    public class Class1
    {
        [CommandMethod("AdskGreeting")]
        public static void AdskGreeting()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl;
                    acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                    OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;

                    // Create a line that starts at 5,5 and ends at 12,3
                    using (Line acLine = new Line(new Point3d(-15, -5, 0),
                                                  new Point3d(12, 15, 0)))
                    {

                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acLine);
                        acTrans.AddNewlyCreatedDBObject(acLine, true);
                    }

                    ViewTableRecord view = acDoc.Editor.GetCurrentView();
                    view.CenterPoint = new Autodesk.AutoCAD.Geometry.Point2d(10, 0);
                    view.Height = 50;
                    view.Width = 50;
                    acDoc.Editor.SetCurrentView(view);

                    // Save the new object to the database
                    acTrans.Commit();
                }

                
            }
        }
    }
}
