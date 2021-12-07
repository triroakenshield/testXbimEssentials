using System.Linq;

using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;

namespace testXbimEssentials
{
    public static class IfcExtension
    {
        public static void AddAggregates(this IfcObjectDefinition obj, IfcObjectDefinition child)
        {
            var ifcRelDecomposes = obj.IsDecomposedBy.FirstOrDefault();
            if (ifcRelDecomposes == null)
            {
                var ifcRelAggregates = obj.Model.Instances.New<IfcRelAggregates>();
                ifcRelAggregates.RelatingObject = obj;
                ifcRelAggregates.RelatedObjects.Add(child);
            }
            else
                ifcRelDecomposes.RelatedObjects.Add(child);
        }

        public static void AddElement(this IfcSpatialStructureElement obj, IfcProduct product)
        {
            var spatialStructure = obj.ContainsElements.FirstOrDefault();
            if (spatialStructure == null) //none defined create the relationship
            {
                var relSe = obj.Model.Instances.New<IfcRelContainedInSpatialStructure>();
                relSe.RelatingStructure = obj;
                relSe.RelatedElements.Add(product);
            }
            else
                spatialStructure.RelatedElements.Add(product);
        }
    }
}