using System;
using Microsoft.AspNet.Mvc;
using Pigeoid.Epsg;
using WebApp.Models;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Vertesaur.Transformation;
using Vertesaur;
using Pigeoid;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers
{
    public class EpsgController : Controller
    {

        public IActionResult Index()
        {
            return View(new EpsgIndex {
                MicroDb = EpsgMicroDatabase.Default
            });
        }

        public IActionResult Crs(int id)
        {
            return View(EpsgMicroDatabase.Default.GetCrs(id));
        }

        public IActionResult OpSearch(int sourceCrsCode, int targetCrsCode)
        {
            var sourceCrs = EpsgMicroDatabase.Default.GetCrs(sourceCrsCode);
            if (sourceCrs == null)
                return new HttpNotFoundResult();
            var targetCrs = EpsgMicroDatabase.Default.GetCrs(targetCrsCode);
            if (targetCrs == null)
                return new HttpNotFoundResult();

            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            var paths = pathGenerator.Generate(sourceCrs, targetCrs);

            var viewModel = new EpsgOpSearchResult {
                SourceCrs = sourceCrs,
                TargetCrs = targetCrs,
                Paths = paths
            };
            return View(viewModel);
        }

        private static object CreateCoordinateOperationInput(ITransformation transformation, double[] values)
        {
            foreach(var validInputType in transformation.GetInputTypes())
            {
                if(validInputType == typeof(Point2) && values.Length == 2)
                {
                    return new Point2(values[0], values[1]);
                }
                if(validInputType == typeof(Point3) && values.Length == 3)
                {
                    return new Point3(values[0], values[1], values[2]);
                }
                if(validInputType == typeof(GeographicCoordinate) && values.Length == 2)
                {
                    return new GeographicCoordinate(values[1], values[0]);
                }
                if(validInputType == typeof(GeographicHeightCoordinate) && values.Length == 3)
                {
                    return new GeographicHeightCoordinate(values[1], values[0], values[2]);
                }
            }
            return null;
        }

        private static double[] CreateCoordinateValues(object geometry)
        {
            if(geometry is Point2)
            {
                var p = (Point2)geometry;
                return new[] { p.X,p.Y};
            }
            if(geometry is Point3)
            {
                var p = (Point3)geometry;
                return new[] { p.X, p.Y, p.Z };
            }
            if(geometry is Vector2)
            {
                var p = (Vector2)geometry;
                return new[] { p.X, p.Y};
            }
            if(geometry is Vector3)
            {
                var p = (Vector3)geometry;
                return new[] { p.X, p.Y, p.Z };
            }
            if(geometry is GeographicCoordinate)
            {
                var p = (GeographicCoordinate)geometry;
                return new[] { p.Longitude, p.Latitude};
            }
            if (geometry is GeographicHeightCoordinate)
            {
                var p = (GeographicHeightCoordinate)geometry;
                return new[] { p.Longitude, p.Latitude, p.Height };
            }
            return null;
        }

        public IActionResult OpExec(int sourceCrsCode, int targetCrsCode, int selectionIndex, double[] inputValues)
        {
            if (inputValues == null)
                throw new ArgumentNullException("input");

            var sourceCrs = EpsgMicroDatabase.Default.GetCrs(sourceCrsCode);
            if (sourceCrs == null)
                throw new ArgumentException("crs not found", "sourceCrsCode");

            var targetCrs = EpsgMicroDatabase.Default.GetCrs(targetCrsCode);
            if (targetCrs == null)
                throw new ArgumentException("crs not found", "targetCrsCode");

            var pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();
            var paths = pathGenerator.Generate(sourceCrs, targetCrs);
            var selectedPath = paths.Skip(selectionIndex).FirstOrDefault();
            if (selectedPath == null)
                throw new ArgumentException("path could not be found");

            var compiler = new StaticCoordinateOperationCompiler();
            var compiledPath = compiler.Compile(selectedPath);
            if (compiledPath == null)
                throw new InvalidOperationException("Could not compile the requested operation path.");

            var inputValue = CreateCoordinateOperationInput(compiledPath, inputValues);
            if (inputValue == null)
                throw new InvalidOperationException("Failed to construct input value.");

            var outputResult = compiledPath.TransformValue(inputValue);
            if (outputResult == null)
                throw new InvalidOperationException("Failed to calculate result value.");

            var outputResultValues = CreateCoordinateValues(outputResult);
            if (outputResultValues == null)
                throw new InvalidOperationException("Failed to convert result values.");

            return new ObjectResult(outputResultValues);
        }

    }
}
