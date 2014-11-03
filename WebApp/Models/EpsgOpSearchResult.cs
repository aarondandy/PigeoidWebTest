using System;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class EpsgOpSearchResult
    {
        public EpsgCrs SourceCrs;
        public EpsgCrs TargetCrs;
        public IEnumerable<ICoordinateOperationCrsPathInfo> Paths;
    }
}