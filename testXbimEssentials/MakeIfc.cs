using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
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
                bs.AddElement(modelHelper.GetSlab());

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