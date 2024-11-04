using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Sample001
{
    internal class Class2
    {
        [CommandMethod("FilterElements")]
        public static void FilterElements()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (DocumentLock acLckDoc = doc.LockDocument())
            {
                doc.Editor.WriteMessage("Filter Element");

                using (Transaction transaction = doc.TransactionManager.StartTransaction())
                {
                    var blockTable = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    var modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                    foreach (var item in modelSpace)
                    {
                        var obj = transaction.GetObject(item, OpenMode.ForRead);
                        if (obj is Autodesk.AutoCAD.DatabaseServices.Line line)
                        {
                            var startPoint = line.StartPoint;
                            var endPoint = line.EndPoint;

                            var layer = transaction.GetObject(line.LayerId, OpenMode.ForRead) as LayerTableRecord;

                            var color = line.Color;
                            var lineWeight = line.LineWeight;
                        }
                        if (obj is Autodesk.AutoCAD.DatabaseServices.Polyline polyLine)
                        {
                            var layer = transaction.GetObject(polyLine.LayerId, OpenMode.ForRead) as LayerTableRecord;

                            var color = polyLine.Color;
                            var lineWeight = polyLine.LineWeight;
                        }
                        if (obj is Autodesk.AutoCAD.DatabaseServices.Arc arc)
                        {
                            var center = arc.Center;
                            var startPoint = arc.StartPoint;
                            var endPoint = arc.EndPoint;
                            var startAngle = arc.StartAngle;
                            var endAngle = arc.EndAngle;
                            var radius = arc.Radius;

                            var color = arc.Color;

                            var layer = transaction.GetObject(arc.LayerId, OpenMode.ForRead) as LayerTableRecord;
                        }
                        if (obj is Autodesk.AutoCAD.DatabaseServices.Circle circle)
                        {
                            var center= circle.Center;
                            var startPoint = circle.Radius;

                            var color = circle.Color;
                            var lineWeight = circle.LineWeight;

                            var layer = transaction.GetObject(circle.LayerId, OpenMode.ForRead) as LayerTableRecord;
                        }
                        if (obj is Autodesk.AutoCAD.DatabaseServices.Spline spline)
                        {
                            var color = spline.Color;

                            

                            var layer = transaction.GetObject(spline.LayerId, OpenMode.ForRead) as LayerTableRecord;
                        }
                        if (obj is Autodesk.AutoCAD.DatabaseServices.BlockReference blockReference)
                        {
                            var color=blockReference.Color;

                            var layer = transaction.GetObject(blockReference.LayerId, OpenMode.ForRead) as LayerTableRecord;
                        }

                        if (obj is Autodesk.AutoCAD.DatabaseServices.DBText mText)
                        {
                            var text = mText.TextString;
                            var angle = mText.Rotation;
                            var loction = mText.Position;

                            var color=mText.Color;
                        }

                    }


                 


                }



            }
        }

    }
}
