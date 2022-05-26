using Mapster.Common.MemoryMappedTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Mapster.Rendering;

public static class TileRenderer
{
    public static BaseShape Tessellate(this MapFeatureData feature, ref BoundingBox boundingBox, ref PriorityQueue<BaseShape, int> shapes)
    {
        BaseShape? baseShape = null;

        var featureType = feature.Type;
        var foundBoundary = false;
        var foundLevel = false;
        bool stoppingCondition = false;
        ReadOnlySpan<Coordinate> coordinates = feature.Coordinates;
        foreach (var entry in feature.Properties)
        {
            switch (entry.Key)
            {
                case MapFeatureData.propertyTypes.highway:
                    if (MapFeature.HighwayTypes.Any(type => entry.Value.StartsWith(type)))
                    {
                        var road = new Road(coordinates);
                        baseShape = road;
                        shapes.Enqueue(road, road.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                case MapFeatureData.propertyTypes.water:
                    if (feature.Type != GeometryType.Point)
                    {
                        var waterway = new Waterway(coordinates, feature.Type == GeometryType.Polygon);
                        baseShape = waterway;
                        shapes.Enqueue(waterway, waterway.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                case MapFeatureData.propertyTypes.boundary:
                    if (entry.Value.StartsWith("forest"))
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        stoppingCondition = true;
                    }
                    else if (entry.Value.StartsWith("administrative"))
                    {
                        foundBoundary = true;
                        if (foundBoundary && foundLevel)
                        {
                            var border = new Border(coordinates);
                            baseShape = border;
                            shapes.Enqueue(border, border.ZIndex);
                            stoppingCondition = true;
                        }
                    }
                    break;

                case MapFeatureData.propertyTypes.admin_level:
                    if (entry.Value == "2")
                    {
                        foundLevel = true;
                        if (foundBoundary && foundLevel)
                        {
                            var border = new Border(coordinates);
                            baseShape = border;
                            shapes.Enqueue(border, border.ZIndex);
                            stoppingCondition = true;
                        }
                    }
                    break;

                case MapFeatureData.propertyTypes.place:
                    if (entry.Value.StartsWith("city") || entry.Value.StartsWith("town") ||
                        entry.Value.StartsWith("locality") || entry.Value.StartsWith("hamlet"))
                    {
                        var popplace = new PopulatedPlace(coordinates, feature);
                        baseShape = popplace;
                        shapes.Enqueue(popplace, popplace.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                case MapFeatureData.propertyTypes.railway:
                    var railway = new Railway(coordinates);
                    baseShape = railway;
                    shapes.Enqueue(railway, railway.ZIndex);
                    stoppingCondition = true;
                    break;

                case MapFeatureData.propertyTypes.natural:
                    if (featureType == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, feature);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                case MapFeatureData.propertyTypes.landuse:
                    if (entry.Value.StartsWith("forest") || entry.Value.StartsWith("orchard"))
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        stoppingCondition = true;
                    }
                    else if (feature.Type == GeometryType.Polygon)
                    {
                        if (entry.Value.StartsWith("residential") || entry.Value.StartsWith("cemetery") ||
                            entry.Value.StartsWith("industrial") || entry.Value.StartsWith("commercial") ||
                            entry.Value.StartsWith("square") || entry.Value.StartsWith("construction") ||
                            entry.Value.StartsWith("military") || entry.Value.StartsWith("quarry") ||
                            entry.Value.StartsWith("brownfield"))
                        {
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                            stoppingCondition = true;
                        }
                        else if (entry.Value.StartsWith("farm") || entry.Value.StartsWith("meadow") ||
                                 entry.Value.StartsWith("grass") || entry.Value.StartsWith("greenfield") ||
                                 entry.Value.StartsWith("recreation_ground") || entry.Value.StartsWith("winter_sports") ||
                                 entry.Value.StartsWith("allotments"))
                        {
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Plain);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                            stoppingCondition = true;
                        }
                        else if (entry.Value.StartsWith("reservoir") || entry.Value.StartsWith("basin"))
                        {
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Water);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                            stoppingCondition = true;
                        }
                    }
                    break;

                case MapFeatureData.propertyTypes.building:
                    if (feature.Type == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                case MapFeatureData.propertyTypes.leisure:
                    if (feature.Type == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                case MapFeatureData.propertyTypes.amenity:
                    if (feature.Type == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        stoppingCondition = true;
                    }
                    break;

                default:
                    break;
            }
            if (stoppingCondition) break;
        }

        if (baseShape != null)
        {
            for (var j = 0; j < baseShape.ScreenCoordinates.Length; ++j)
            {
                boundingBox.MinX = Math.Min(boundingBox.MinX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MaxX = Math.Max(boundingBox.MaxX, baseShape.ScreenCoordinates[j].X);
                boundingBox.MinY = Math.Min(boundingBox.MinY, baseShape.ScreenCoordinates[j].Y);
                boundingBox.MaxY = Math.Max(boundingBox.MaxY, baseShape.ScreenCoordinates[j].Y);
            }
        }

        return baseShape;
    }

    public static Image<Rgba32> Render(this PriorityQueue<BaseShape, int> shapes, BoundingBox boundingBox, int width, int height)
    {
        var canvas = new Image<Rgba32>(width, height);

        // Calculate the scale for each pixel, essentially applying a normalization
        var scaleX = canvas.Width / (boundingBox.MaxX - boundingBox.MinX);
        var scaleY = canvas.Height / (boundingBox.MaxY - boundingBox.MinY);
        var scale = Math.Min(scaleX, scaleY);

        // Background Fill
        canvas.Mutate(x => x.Fill(Color.White));
        while (shapes.Count > 0)
        {
            var entry = shapes.Dequeue();
            entry.TranslateAndScale(boundingBox.MinX, boundingBox.MinY, scale, canvas.Height);
            canvas.Mutate(x => entry.Render(x));
        }

        return canvas;
    }

    public struct BoundingBox
    {
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }
}
