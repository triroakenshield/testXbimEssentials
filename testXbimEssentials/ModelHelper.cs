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
                    _point000 = _model.Instances.New<IfcCartesianPoint>(p =>
                    {
                        p.X = 0;
                        p.Y = 0;
                        p.Z = 0;
                    });
                }
                return _point000;
            }
        }

        private IfcDirection _direction10;

        private IfcDirection Direction10
        {
            get
            {
                if (_direction10 == null)
                {
                    _direction10 = _model.Instances.New<IfcDirection>(d =>
                    {
                        d.X = 1;
                        d.Y = 0;
                    });
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
                    _direction01 = _model.Instances.New<IfcDirection>(d =>
                    {
                        d.X = 0;
                        d.Y = 1;
                    });
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

        private IfcShapeRepresentation GetSweptSolid(double xDim, double yDim, double depth)
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
                s.Position = WorldCoordinateSystem;
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
            var shape = _model.Instances.New<IfcProductDefinitionShape>();
            shape.Representations.Add(GetSweptSolid(1000, 1000, 100));

            return _model.Instances.New<IfcSlab>(s =>
            {
                s.Name = "slab";
                s.ObjectPlacement = Local;
                s.Representation = shape;
                s.PredefinedType = IfcSlabTypeEnum.BASESLAB;
            });
        }

        public void Dispose()
        {
            _model = null;
        }
    }
}