using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.IO;

namespace testXbimEssentials
{
    public class MakeIfc
    {
        private void Make(string ifcPath)
        {
            var ec = new XbimEditorCredentials();
            using var model = IfcStore.Create(ec, XbimSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);
            using (var tr = model.BeginTransaction("Make"))
            {
                using var modelHelper = new ModelHelper(model);

                var prj = model.Instances.New<IfcProject>(a => a.Name = "Project #1");
                //prj.GlobalId = new IfcGloballyUniqueId("abcdefghijklmnopqrs101");
                //prj.OwnerHistory = null; 
                prj.UnitsInContext = modelHelper.GetUnitAssignment();
                prj.RepresentationContexts.Add(modelHelper.ModelContext);

                var b = modelHelper.GetBuilding();
                prj.AddBuilding(b);

                var bs = modelHelper.GetBuildingStorey();
                //b.BuildingStoreys.ad
                b.AddAggregates(bs);

                var roofShape = modelHelper.GetShape(modelHelper
                    .GetSweptSolid(modelHelper.WorldCoordinateSystem, 1000, 1000, 100));

                bs.AddElement(modelHelper.GetSlab("slab", modelHelper.Local, roofShape, IfcSlabTypeEnum.BASESLAB));

                var wall1 = modelHelper.GetWall("wall1",
                    modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(500, 0, 100), 100, 1000, 1000));

                bs.AddElement(wall1);

                var wall2 = modelHelper.GetWall("wall2",
                    modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(-500, 0, 100), 100, 1000, 1000));

                bs.AddElement(wall2);

                var wall3 = modelHelper.GetWall("wall3",
                    modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(0, -500, 100), 1000, 100, 1000));

                bs.AddElement(wall3);

                var wall4 = modelHelper.GetWall("wall4",
                    modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(0, 500, 100), 1000, 100, 1000));

                bs.AddElement(wall4);

                var brep = modelHelper.GetBrep();
                var doorShape = modelHelper.GetShape(brep);
                var localPlacement = modelHelper.GetLocalPlacement(-100, 400, 100);

                var door = modelHelper.GetDoor("door", localPlacement, doorShape);

                bs.AddElement(door);

                //var open1 = modelHelper.GetOpeningElement(localPlacement, doorShape);
                //modelHelper.GetVoidsElement(wall4, open1);
                //modelHelper.GetFillsElement(door, open1);

                modelHelper.GetOpeningElement(wall4, door);

                var windowLocalPlacement = modelHelper.GetLocalPlacement(-100, -600, 400);
                var window = modelHelper.GetWindow("window", windowLocalPlacement, doorShape);

                bs.AddElement(window);
                modelHelper.GetOpeningElement(wall3, window);

                var roofLocalPlacement = modelHelper.GetLocalPlacement(modelHelper.Local, 0, 0, 1100);
                var roof = modelHelper.GetRoof("roof", roofLocalPlacement, null, IfcRoofTypeEnum.FLAT_ROOF);
                bs.AddElement(roof);

                var roofSlab = modelHelper.GetSlab("roofSlab", roofLocalPlacement, roofShape, IfcSlabTypeEnum.ROOF);
                roof.AddAggregates(roofSlab);

                //IfcPropertySet

                var propertySet = modelHelper.GetPropertySet("test_ps");
                propertySet.HasProperties.Add(modelHelper.GetPropertySingleValue("p1", new IfcBoolean(true)));
                propertySet.HasProperties.Add(modelHelper.GetPropertySingleValue("p2", new IfcLabel("testLabelProperty")));

                var relDefinesByProperties = modelHelper.GetRelDefinesByProperties();
                relDefinesByProperties.RelatingPropertyDefinition = propertySet;
                relDefinesByProperties.RelatedObjects.Add(door);

                tr.Commit();
            }
            model.SaveAs(ifcPath);
        }

        public void SaveIfc(string ifcPath)
        {
            Make(ifcPath);
        }
    }
}