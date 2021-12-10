using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.IO;
// ReSharper disable CommentTypo

namespace testXbimEssentials
{
    public class MakeIfc
    {
        private void Make(string ifcPath)
        {
            // Объект XbimEditorCredentials отвечает за IfcOwnerHistory и связанные
            // с ним IfcPersonAndOrganization, IfcPerson, IfcOrganization, IfcApplication
            var ec = new XbimEditorCredentials();
            using var model = IfcStore.Create(ec, XbimSchemaVersion.Ifc2X3, XbimStoreType.InMemoryModel);
            using (var tr = model.BeginTransaction("Make"))
            {
                using var modelHelper = new ModelHelper(model);

                //Добавляем IfcProject, вместе с ним получим IfcOwnerHistory из XbimEditorCredentials
                /*
#1=IFCPROJECT(<GlobalId>,#2,'Project #1',$,$,$,$,$,$);
#2=IFCOWNERHISTORY(#5,#6,$,.ADDED.,1639091796,$,$,0);
#3=IFCPERSON($,$,$,$,$,$,$,$);
#4=IFCORGANIZATION($,$,$,$,$);
#5=IFCPERSONANDORGANIZATION(#3,#4,$);
#6=IFCAPPLICATION(#4,$,$,$);
                 */
                var prj = model.Instances.New<IfcProject>(a => a.Name = "Project #1");
                //prj.GlobalId = new IfcGloballyUniqueId("abcdefghijklmnopqrs101");
                //prj.OwnerHistory = null;

                //Добавляем IfcUnitAssignment и несколько IfcSIUnit с ним
                /*
#7=IFCSIUNIT(*,.LENGTHUNIT.,.MILLI.,.METRE.);
#8=IFCSIUNIT(*,.AREAUNIT.,$,.SQUARE_METRE.);
#9=IFCSIUNIT(*,.VOLUMEUNIT.,$,.CUBIC_METRE.);
#10=IFCSIUNIT(*,.PLANEANGLEUNIT.,$,.RADIAN.);
#11=IFCUNITASSIGNMENT((#7,#8,#9,#10));
                 */
                prj.UnitsInContext = modelHelper.GetUnitAssignment();

                //Добавляем IfcGeometricRepresentationContext и вместе с ним создаём WorldCoordinateSystem (IfcAxis2Placement3D) 
                /*
#12=IFCGEOMETRICREPRESENTATIONCONTEXT($,'Model',3,0.01,#13,#15);
#13=IFCAXIS2PLACEMENT3D(#14,$,$);
#14=IFCCARTESIANPOINT((0.,0.,0.));
#15=IFCDIRECTION((0.,1.));
                 */
                prj.RepresentationContexts.Add(modelHelper.ModelContext);

                //Добавляем IfcBuilding и вместе с ним создаём IfcLocalPlacement
                /*
#16=IFCBUILDING('2EjE5$8A93IOLMcMQb54V6',#2,$,$,$,#17,$,$,.ELEMENT.,$,$,$);
#17=IFCLOCALPLACEMENT($,#13);
                */
                var b = modelHelper.GetBuilding();
                //Привязываем IfcBuilding к IfcProject и создаём IfcRelAggregates
                /*
#18=IFCRELAGGREGATES(<GlobalId>,#2,$,$,#1,(#16));
                */
                prj.AddBuilding(b);

                //Добавляем IfcBuildingStorey
                /*
#19=IFCBUILDINGSTOREY(<GlobalId>,#2,$,'level1',$,#17,$,$,.ELEMENT.,$);
                */
                var bs = modelHelper.GetBuildingStorey();
                //Привязываем IfcBuildingStorey к IfcBuilding и создаём IfcRelAggregates
                /*
#19=IFCBUILDINGSTOREY(<GlobalId>,#2,$,'level1',$,#17,$,$,.ELEMENT.,$);
                */
                b.AddAggregates(bs);

                //Создаём геометрию для пола и крыши в виде IfcShapeRepresentation
                /*
#21=IFCRECTANGLEPROFILEDEF(.AREA.,$,#22,1000.,1000.);
#22=IFCAXIS2PLACEMENT2D(#23,#24);
#23=IFCCARTESIANPOINT((0.,0.));
#24=IFCDIRECTION((1.,0.));
#25=IFCEXTRUDEDAREASOLID(#21,#13,#26,100.);
#26=IFCDIRECTION((0.,0.,1.));
#27=IFCSHAPEREPRESENTATION(#12,'Body','SweptSolid',(#25));
                */
                var roofShape = modelHelper.GetShape(modelHelper.GetSweptSolid(
                        modelHelper.WorldCoordinateSystem, 1000, 1000, 100));

                //Создаём пол в виде IfcSlab и добавляем его к IfcBuildingStorey
                /*
#28=IFCPRODUCTDEFINITIONSHAPE($,$,(#27));
#29=IFCSLAB(<GlobalId>,#2,'slab',$,$,#17,#28,$,.BASESLAB.);
#30=IFCRELCONTAINEDINSPATIALSTRUCTURE(<GlobalId>,#2,$,$,(#29),#19);
                */
                bs.AddElement(modelHelper.GetSlab("slab", modelHelper.Local, roofShape, IfcSlabTypeEnum.BASESLAB));

                //Создаём стену wall1 (IfcWallStandardCase)
                /*
#31=IFCAXIS2PLACEMENT3D(#32,$,$);
#32=IFCCARTESIANPOINT((500.,0.,100.));
#33=IFCRECTANGLEPROFILEDEF(.AREA.,$,#22,100.,1000.);
#34=IFCEXTRUDEDAREASOLID(#33,#31,#26,1000.);
#35=IFCSHAPEREPRESENTATION(#12,'Body','SweptSolid',(#34));
#36=IFCWALLSTANDARDCASE(<GlobalId>,#2,'wall1',$,$,#17,#37,$);
#37=IFCPRODUCTDEFINITIONSHAPE($,$,(#35));
                */
                var wall1 = modelHelper.GetWall("wall1", modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(500, 0, 100), 100, 1000, 1000));

                //Добавляем wall1 к IfcBuildingStorey
                bs.AddElement(wall1);

                //Создаём стену wall2 (IfcWallStandardCase)
                /*
#38=IFCAXIS2PLACEMENT3D(#39,$,$);
#39=IFCCARTESIANPOINT((-500.,0.,100.));
#40=IFCRECTANGLEPROFILEDEF(.AREA.,$,#22,100.,1000.);
#41=IFCEXTRUDEDAREASOLID(#40,#38,#26,1000.);
#42=IFCSHAPEREPRESENTATION(#12,'Body','SweptSolid',(#41));
#43=IFCWALLSTANDARDCASE(<GlobalId>,#2,'wall2',$,$,#17,#44,$);
#44=IFCPRODUCTDEFINITIONSHAPE($,$,(#42));
                */
                var wall2 = modelHelper.GetWall("wall2", modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(-500, 0, 100), 100, 1000, 1000));

                //Добавляем wall2 к IfcBuildingStorey
                bs.AddElement(wall2);

                //Создаём стену wall3 (IfcWallStandardCase)
                /*
#45=IFCAXIS2PLACEMENT3D(#46,$,$);
#46=IFCCARTESIANPOINT((0.,-500.,100.));
#47=IFCRECTANGLEPROFILEDEF(.AREA.,$,#22,1000.,100.);
#48=IFCEXTRUDEDAREASOLID(#47,#45,#26,1000.);
#49=IFCSHAPEREPRESENTATION(#12,'Body','SweptSolid',(#48));
#50=IFCWALLSTANDARDCASE(<GlobalId>,#2,'wall3',$,$,#17,#51,$);
#51=IFCPRODUCTDEFINITIONSHAPE($,$,(#49));
                */
                var wall3 = modelHelper.GetWall("wall3", modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(0, -500, 100), 1000, 100, 1000));

                //Добавляем wall3 к IfcBuildingStorey
                bs.AddElement(wall3);

                //Создаём стену wall4 (IfcWallStandardCase)
                /*
#52=IFCAXIS2PLACEMENT3D(#53,$,$);
#53=IFCCARTESIANPOINT((0.,500.,100.));
#54=IFCRECTANGLEPROFILEDEF(.AREA.,$,#22,1000.,100.);
#55=IFCEXTRUDEDAREASOLID(#54,#52,#26,1000.);
#56=IFCSHAPEREPRESENTATION(#12,'Body','SweptSolid',(#55));
#57=IFCWALLSTANDARDCASE(<GlobalId>,#2,'wall4',$,$,#17,#58,$);
#58=IFCPRODUCTDEFINITIONSHAPE($,$,(#56));
                */
                var wall4 = modelHelper.GetWall("wall4", modelHelper.GetSweptSolid(
                        modelHelper.GetAxis(0, 500, 100), 1000, 100, 1000));

                //Добавляем wall4 к IfcBuildingStorey
                bs.AddElement(wall4);

                //Создаём IfcFacetedBrep в виде IfcShapeRepresentation и будем использовать эту геометрию для двери и окна
                /*
#59=IFCCARTESIANPOINT((0.,0.,0.));
#60=IFCCARTESIANPOINT((200.,0.,0.));
#61=IFCCARTESIANPOINT((200.,200.,0.));
#62=IFCCARTESIANPOINT((0.,200.,0.));
#63=IFCCARTESIANPOINT((0.,0.,500.));
#64=IFCCARTESIANPOINT((200.,0.,500.));
#65=IFCCARTESIANPOINT((200.,200.,500.));
#66=IFCCARTESIANPOINT((0.,200.,500.));
#67=IFCCLOSEDSHELL((#70,#73,#76,#79,#82,#85));
#68=IFCPOLYLOOP((#59,#60,#61,#62));
#69=IFCFACEOUTERBOUND(#68,.T.);
#70=IFCFACE((#69));
#71=IFCPOLYLOOP((#63,#64,#65,#66));
#72=IFCFACEOUTERBOUND(#71,.T.);
#73=IFCFACE((#72));
#74=IFCPOLYLOOP((#59,#60,#64,#63));
#75=IFCFACEOUTERBOUND(#74,.T.);
#76=IFCFACE((#75));
#77=IFCPOLYLOOP((#60,#61,#65,#64));
#78=IFCFACEOUTERBOUND(#77,.T.);
#79=IFCFACE((#78));
#80=IFCPOLYLOOP((#61,#62,#66,#65));
#81=IFCFACEOUTERBOUND(#80,.T.);
#82=IFCFACE((#81));
#83=IFCPOLYLOOP((#62,#59,#63,#66));
#84=IFCFACEOUTERBOUND(#83,.T.);
#85=IFCFACE((#84));
#86=IFCFACETEDBREP(#67);
#87=IFCSHAPEREPRESENTATION(#12,'Body','Brep',(#86));
                */
                var brep = modelHelper.GetBrep();

                //Создаём IfcProductDefinitionShape для двери
                /*
#88=IFCPRODUCTDEFINITIONSHAPE($,$,(#87));
                */
                var doorShape = modelHelper.GetShape(brep);

                //Создаём IfcLocalPlacement для определения позиции размещения двери
                /*
#89=IFCLOCALPLACEMENT($,#90);
#90=IFCAXIS2PLACEMENT3D(#91,$,$);
#91=IFCCARTESIANPOINT((-100.,400.,100.));
                */
                var localPlacement = modelHelper.GetLocalPlacement(-100, 400, 100);

                //Создаём дверь (IfcDoor)
                /*
#92=IFCDOOR(<GlobalId>,#2,'door',$,$,#89,#88,$,$,$);
                */
                var door = modelHelper.GetDoor("door", localPlacement, doorShape);

                //Добавляем door к IfcBuildingStorey
                bs.AddElement(door);

                //Создаём отверстие для двери
                /*
#93=IFCOPENINGELEMENT(<GlobalId>,#2,$,$,$,#89,#88,$);
#94=IFCRELVOIDSELEMENT(<GlobalId>,#2,$,$,#57,#93);
#95=IFCRELFILLSELEMENT(<GlobalId>,#2,$,$,#93,#92);
                */
                modelHelper.GetOpeningElement(wall4, door);

                //Создаём IfcLocalPlacement для определения позиции размещения окна
                /*
#96=IFCLOCALPLACEMENT($,#97);
#97=IFCAXIS2PLACEMENT3D(#98,$,$);
#98=IFCCARTESIANPOINT((-100.,-600.,400.));
                 */
                var windowLocalPlacement = modelHelper.GetLocalPlacement(-100, -600, 400);

                //Создаём окно (IfcWindow)
                /*
#99=IFCWINDOW(<GlobalId>,#2,'window',$,$,#96,#88,$,$,$);
                */
                var window = modelHelper.GetWindow("window", windowLocalPlacement, doorShape);

                //Добавляем window к IfcBuildingStorey
                bs.AddElement(window);

                //Создаём отверстие для окна
                /*
#100=IFCOPENINGELEMENT(<GlobalId>,#2,$,$,$,#96,#88,$);
#101=IFCRELVOIDSELEMENT(<GlobalId>,#2,$,$,#50,#100);
#102=IFCRELFILLSELEMENT(<GlobalId>,#2,$,$,#100,#99);
                */
                modelHelper.GetOpeningElement(wall3, window);

                //Создаём IfcLocalPlacement для определения позиции размещения крыши
                /*
#103=IFCLOCALPLACEMENT(#17,#104);
#104=IFCAXIS2PLACEMENT3D(#105,$,$);
#105=IFCCARTESIANPOINT((0.,0.,1100.));
                */
                var roofLocalPlacement = modelHelper.GetLocalPlacement(modelHelper.Local, 0, 0, 1100);

                //Создаём крышу (IfcRoof)
                /*
#106=IFCROOF(<GlobalId>,#2,'roof',$,$,#103,$,$,.FLAT_ROOF.);
                */
                var roof = modelHelper.GetRoof("roof", roofLocalPlacement, null, IfcRoofTypeEnum.FLAT_ROOF);

                //Добавляем roof к IfcBuildingStorey
                bs.AddElement(roof);

                //Добавляем перекрытие в крышу (IfcSlab)
                /*
#107=IFCSLAB(<GlobalId>,#2,'roofSlab',$,$,#103,#28,$,.ROOF.);
                */
                var roofSlab = modelHelper.GetSlab("roofSlab", roofLocalPlacement, roofShape, IfcSlabTypeEnum.ROOF);
                //Добавляем roofSlab к roof
                /*
#108=IFCRELAGGREGATES(<GlobalId>,#2,$,$,#106,(#107));
                */
                roof.AddAggregates(roofSlab);

                //Добавляем набор свойств (IfcPropertySet)
                /*
#109=IFCPROPERTYSET(<GlobalId>,#2,'Pset_test_ps',$,(#110,#111));
                */
                var propertySet = modelHelper.GetPropertySet("test_ps");

                //Добавляем свойства (IfcPropertySingleValue)
                /*
#110=IFCPROPERTYSINGLEVALUE('p1',$,IFCBOOLEAN(.T.),$);
                */
                propertySet.HasProperties.Add(modelHelper.GetPropertySingleValue("p1", new IfcBoolean(true)));

                /*
#111=IFCPROPERTYSINGLEVALUE('p2',$,IFCLABEL('testLabelProperty'),$);
                */
                propertySet.HasProperties.Add(modelHelper.GetPropertySingleValue("p2", new IfcLabel("testLabelProperty")));
                
                //Связываем свойства с дверью
                /*
#112=IFCRELDEFINESBYPROPERTIES(<GlobalId>,#2,$,$,(#92),#109);
                */
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