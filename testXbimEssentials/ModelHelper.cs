using System;

using Xbim.Common;
using Xbim.Ifc2x3.GeometricConstraintResource;
using Xbim.Ifc2x3.GeometricModelResource;
using Xbim.Ifc2x3.GeometryResource;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.ProfileResource;
using Xbim.Ifc2x3.RepresentationResource;
using Xbim.Ifc2x3.SharedBldgElements;
using Xbim.Ifc2x3.TopologyResource;
//using IfcFeatureElementSubtraction = Xbim.Ifc4.ProductExtension.IfcFeatureElementSubtraction;

namespace testXbimEssentials
{
    public class ModelHelper : IDisposable
    {
        private IModel _model;

        private IfcCartesianPoint _point00;

        private IfcCartesianPoint Point00
        {
            get
            {
                if (_point00 == null)
                {
                    _point00 = _model.Instances.New<IfcCartesianPoint>(p =>
                    {
                        p.X = 0;
                        p.Y = 0;
                    });
                }
                return _point00;
            }
        }

        private IfcCartesianPoint _point000;

        private IfcCartesianPoint Point000
        {
            get
            {
                if (_point000 == null)
                {
                    _point000 = GetPoint3D(0, 0, 0);
                }
                return _point000;
            }
        }

        public IfcDirection GetDirection2D(double x, double y)
        {
            return _model.Instances.New<IfcDirection>(d =>
            {
                d.X = x;
                d.Y = y;
            });
        }

        private IfcDirection _direction10;

        private IfcDirection Direction10
        {
            get
            {
                if (_direction10 == null)
                {
                    _direction10 = GetDirection2D(1, 0);
                }
                return _direction10;
            }
        }

        private IfcDirection _direction01;

        private IfcDirection Direction01
        {
            get
            {
                if (_direction01 == null)
                {
                    _direction01 = GetDirection2D(0, 1);
                }
                return _direction01;
            }
        }

        private IfcDirection _direction001;

        private IfcDirection Direction001
        {
            get
            {
                if (_direction001 == null)
                {
                    _direction001 = _model.Instances.New<IfcDirection>(d =>
                    {
                        d.X = 0;
                        d.Y = 0;
                        d.Z = 1;
                    });
                }
                return _direction001;
            }
        }

        private IfcAxis2Placement3D _worldCoordinateSystem;

        private IfcAxis2Placement3D WorldCoordinateSystem
        {
            get
            {
                if (_worldCoordinateSystem == null)
                {
                    _worldCoordinateSystem = _model.Instances.New<IfcAxis2Placement3D>(a
                        => a.Location = Point000);
                }
                return _worldCoordinateSystem;
            }
        }

        private IfcAxis2Placement2D _axis2Dp00d10;

        private IfcAxis2Placement2D Axis2Dp00d10
        {
            get
            {
                if (_axis2Dp00d10 == null)
                {
                    _axis2Dp00d10 = _model.Instances.New<IfcAxis2Placement2D>(a =>
                    {
                        a.Location = Point00;
                        a.RefDirection = Direction10;
                    });
                }
                return _axis2Dp00d10;
            }
        }

        public ModelHelper(IModel model)
        {
            _model = model;
        }

        public IfcUnitAssignment GetUnitAssignment()
        {
            var u2 = _model.Instances.New<IfcSIUnit>(s =>
            {
                s.UnitType = IfcUnitEnum.LENGTHUNIT;
                s.Prefix = IfcSIPrefix.MILLI;
                s.Name = IfcSIUnitName.METRE;
            });

            var u3 = _model.Instances.New<IfcSIUnit>(s =>
            {
                s.UnitType = IfcUnitEnum.AREAUNIT;
                s.Name = IfcSIUnitName.SQUARE_METRE;
            });

            var u4 = _model.Instances.New<IfcSIUnit>(s =>
            {
                s.UnitType = IfcUnitEnum.VOLUMEUNIT;
                s.Name = IfcSIUnitName.CUBIC_METRE;
            });

            var u5 = _model.Instances.New<IfcSIUnit>(s =>
            {
                s.UnitType = IfcUnitEnum.PLANEANGLEUNIT;
                s.Name = IfcSIUnitName.RADIAN;
            });

            var ua = _model.Instances.New<IfcUnitAssignment>();
            ua.Units.Add(u2);
            ua.Units.Add(u3);
            ua.Units.Add(u4);
            ua.Units.Add(u5);

            return ua;
        }

        private IfcGeometricRepresentationContext GetContext()
        {
            return _model.Instances.New<IfcGeometricRepresentationContext>(c =>
            {
                c.ContextType = "Model";
                c.CoordinateSpaceDimension = 3;
                c.Precision = 0.01;
                c.WorldCoordinateSystem = WorldCoordinateSystem;
                c.TrueNorth = Direction01;
            });
        }

        private IfcGeometricRepresentationContext _modelContext;

        public IfcGeometricRepresentationContext ModelContext
        {
            get
            {
                if (_modelContext == null)
                {
                    _modelContext = GetContext();
                }
                return _modelContext;
            }
        }

        private IfcLocalPlacement _local;

        private IfcLocalPlacement Local
        {
            get
            {
                if (_local == null)
                {
                    _local = _model.Instances.New<IfcLocalPlacement>(l
                        => l.RelativePlacement = _worldCoordinateSystem);
                }
                return _local;
            }
        }

        public IfcBuilding GetBuilding()
        {
            return _model.Instances.New<IfcBuilding>(b =>
            {
                b.CompositionType = IfcElementCompositionEnum.ELEMENT;
                b.ObjectPlacement = Local;
            });
        }

        public IfcBuildingStorey GetBuildingStorey()
        {
            return _model.Instances.New<IfcBuildingStorey>(s =>
            {
                s.Description = "level1";
                s.ObjectPlacement = Local;
                s.CompositionType = IfcElementCompositionEnum.ELEMENT;
            });
        }

        public IfcShapeRepresentation GetSweptSolid(IfcAxis2Placement3D position, double xDim, double yDim, double depth)
        {
            var prof = _model.Instances.New<IfcRectangleProfileDef>(p =>
            {
                p.ProfileType = IfcProfileTypeEnum.AREA;
                p.Position = Axis2Dp00d10;
                p.XDim = xDim;
                p.YDim = yDim;
            });

            var solid = _model.Instances.New<IfcExtrudedAreaSolid>(s =>
            {
                s.SweptArea = prof;
                s.Position = position;
                s.ExtrudedDirection = Direction001;
                s.Depth = depth;
            });

            var body = _model.Instances.New<IfcShapeRepresentation>(s =>
            {
                s.ContextOfItems = ModelContext;
                s.RepresentationIdentifier = "Body";
                s.RepresentationType = "SweptSolid";
            });

            body.Items.Add(solid);

            return body;
        }

        public IfcSlab GetSlab()
        {
            return _model.Instances.New<IfcSlab>(s =>
            {
                s.Name = "slab";
                s.ObjectPlacement = Local;
                s.Representation = GetShape(GetSweptSolid(WorldCoordinateSystem, 1000, 1000, 100));
                s.PredefinedType = IfcSlabTypeEnum.BASESLAB;
            });
        }

        public IfcProductDefinitionShape GetShape(IfcShapeRepresentation representation)
        {
            var shape = _model.Instances.New<IfcProductDefinitionShape>();
            shape.Representations.Add(representation);
            return shape;
        }

        public IfcAxis2Placement3D GetAxis(double x, double y, double z)
        {
            return _model.Instances.New<IfcAxis2Placement3D>(a => a.Location = GetPoint3D(x, y, z));
        }

        public IfcWallStandardCase GetWall(string name, IfcShapeRepresentation shape)
        {
            return _model.Instances.New<IfcWallStandardCase>(w =>
            {
                w.Name = name;
                w.ObjectPlacement = Local;
                w.Representation = GetShape(shape);
            });
        }

        public IfcCartesianPoint GetPoint3D(double x, double y, double z)
        {
            return _model.Instances.New<IfcCartesianPoint>(p =>
            {
                p.X = x;
                p.Y = y;
                p.Z = z;
            });
        }

        public IfcPolyLoop GetPolyLoop(IfcCartesianPoint p0, IfcCartesianPoint p1, IfcCartesianPoint p2,
            IfcCartesianPoint p3)
        {
            var loop = _model.Instances.New<IfcPolyLoop>();
            loop.Polygon.Add(p0);
            loop.Polygon.Add(p1);
            loop.Polygon.Add(p2);
            loop.Polygon.Add(p3);
            return loop;
        }

        public IfcFace GetFace(IfcCartesianPoint p0, IfcCartesianPoint p1, IfcCartesianPoint p2,
            IfcCartesianPoint p3)
        {
            var loop = GetPolyLoop(p0, p1, p2, p3);
            var bound = _model.Instances.New<IfcFaceOuterBound>(b =>
            {
                b.Bound = loop;
                b.Orientation = true;
            });
            return _model.Instances.New<IfcFace>(f => f.Bounds.Add(bound));
        }

        public IfcShapeRepresentation GetBrep()
        {
            var p60 = GetPoint3D(0, 0, 0);
            var p61 = GetPoint3D(200, 0, 0);
            var p62 = GetPoint3D(200, 200, 0);
            var p63 = GetPoint3D(0, 200, 0);
            var p64 = GetPoint3D(0, 0, 500);
            var p65 = GetPoint3D(200, 0, 500);
            var p66 = GetPoint3D(200, 200, 500);
            var p67 = GetPoint3D(0, 200, 500);

            var shell = _model.Instances.New<IfcClosedShell>();
            shell.CfsFaces.Add(GetFace(p60, p61, p62, p63));
            shell.CfsFaces.Add(GetFace(p64, p65, p66, p67));
            shell.CfsFaces.Add(GetFace(p60, p61, p65, p64));
            shell.CfsFaces.Add(GetFace(p61, p62, p66, p65));
            shell.CfsFaces.Add(GetFace(p62, p63, p67, p66));
            shell.CfsFaces.Add(GetFace(p63, p60, p64, p67));

            var brep = _model.Instances.New<IfcFacetedBrep>(b => b.Outer = shell);

            var body = _model.Instances.New<IfcShapeRepresentation>(s =>
            {
                s.ContextOfItems = ModelContext;
                s.RepresentationIdentifier = "Body";
                s.RepresentationType = "Brep";
            });

            body.Items.Add(brep);

            return body;
        }

        public IfcLocalPlacement GetLocalPlacement(double x, double y, double z)
        {
            return _model.Instances.New<IfcLocalPlacement>(l
                => l.RelativePlacement = GetAxis(x, y, z));
        }

        public IfcDoor GetDoor(string name, IfcLocalPlacement local, IfcProductDefinitionShape shape)
        {
            return _model.Instances.New<IfcDoor>(w =>
            {
                w.Name = name;
                w.ObjectPlacement = local;
                w.Representation = shape;
            });
        }

        public IfcOpeningElement GetOpeningElement(IfcObjectPlacement local, IfcProductRepresentation shape)
        {
            return _model.Instances.New<IfcOpeningElement>(o =>
            {
                o.ObjectPlacement = local;
                o.Representation = shape;
            });
        }

        public IfcRelVoidsElement GetVoidsElement(IfcBuildingElement elem, IfcFeatureElementSubtraction open)
        {
            return _model.Instances.New<IfcRelVoidsElement>(v =>
            {
                v.RelatingBuildingElement = elem;
                v.RelatedOpeningElement = open;
            });
        }

        public IfcRelFillsElement GetFillsElement(IfcBuildingElement elem, IfcOpeningElement open)
        {
            return _model.Instances.New<IfcRelFillsElement>(f =>
            {
                f.RelatedBuildingElement = elem;
                f.RelatingOpeningElement = open;
            } );
        }

        public IfcOpeningElement GetOpeningElement(IfcBuildingElement host, IfcBuildingElement client)
        {
            var open1 = GetOpeningElement(client.ObjectPlacement, client.Representation);
            GetVoidsElement(host, open1);
            GetFillsElement(client, open1);
            return open1;
        }

        public void Dispose()
        {
            _model = null;
        }
    }
}