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
        ReadOnlySpan<Coordinate> coordinates = feature.Coordinates;
        foreach (var entry in feature.Properties)
        {
            switch (entry.Key)
            {
                case MapFeatureData.propertyTypes.highway:
                    if (((int)entry.Value.propertiesValues >= 26 && (int)entry.Value.propertiesValues <= 32) || (int)entry.Value.propertiesValues == 48)
                    {
                        var road = new Road(coordinates);
                        baseShape = road;
                        shapes.Enqueue(road, road.ZIndex);
                    }
                    break;

                case MapFeatureData.propertyTypes.water:
                    if (feature.Type != GeometryType.Point)
                    {
                        var waterway = new Waterway(coordinates, feature.Type == GeometryType.Polygon);
                        baseShape = waterway;
                        shapes.Enqueue(waterway, waterway.ZIndex);
                    }
                    break;

                case MapFeatureData.propertyTypes.boundary:
                    if (entry.Value.propertiesValues == MapFeatureData.propertyValuesStruct.propertyValues.forest)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                    }
                    else if (entry.Value.propertiesValues == MapFeatureData.propertyValuesStruct.propertyValues.administrative)
                    {
                        foundBoundary = true;
                        if (foundBoundary && foundLevel)
                        {
                            var border = new Border(coordinates);
                            baseShape = border;
                            shapes.Enqueue(border, border.ZIndex);
                        }
                    }
                    break;

                case MapFeatureData.propertyTypes.admin_level:
                    if (entry.Value.propertiesValues == MapFeatureData.propertyValuesStruct.propertyValues.two)
                    {
                        foundLevel = true;
                        if (foundBoundary && foundLevel)
                        {
                            var border = new Border(coordinates);
                            baseShape = border;
                            shapes.Enqueue(border, border.ZIndex);
                        }
                    }
                    break;

                case MapFeatureData.propertyTypes.place:
                    if ((int)entry.Value.propertiesValues >= 3 && (int)entry.Value.propertiesValues <= 6)
                    {
                        var popplace = new PopulatedPlace(coordinates, feature);
                        baseShape = popplace;
                        shapes.Enqueue(popplace, popplace.ZIndex);
                    }
                    break;

                case MapFeatureData.propertyTypes.railway:
                    var railway = new Railway(coordinates);
                    baseShape = railway;
                    shapes.Enqueue(railway, railway.ZIndex);
                    break;

                case MapFeatureData.propertyTypes.natural:
                    if (featureType == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, feature);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                    }
                    break;

                case MapFeatureData.propertyTypes.landuse:
                    if (entry.Value.propertiesValues == MapFeatureData.propertyValuesStruct.propertyValues.forest ||
                        entry.Value.propertiesValues == MapFeatureData.propertyValuesStruct.propertyValues.orchard)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Forest);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                    }
                    else if (feature.Type == GeometryType.Polygon)
                    {
                        if (((int)entry.Value.propertiesValues >= 8 && (int)entry.Value.propertiesValues <= 15) || (int)entry.Value.propertiesValues == 48)
                        {
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }
                        else if ((int)entry.Value.propertiesValues >= 16 && (int)entry.Value.propertiesValues <= 23)
                        {
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Plain);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }
                        else if (((int)entry.Value.propertiesValues >= 24 && (int)entry.Value.propertiesValues <= 25) || (int)entry.Value.propertiesValues == 49)
                        {
                            var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Water);
                            baseShape = geoFeature;
                            shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                        }
                    }
                    break;

                case MapFeatureData.propertyTypes.building:
                    if (feature.Type == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                    }
                    break;

                case MapFeatureData.propertyTypes.leisure:
                    if (feature.Type == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                    }
                    break;

                case MapFeatureData.propertyTypes.amenity:
                    if (feature.Type == GeometryType.Polygon)
                    {
                        var geoFeature = new GeoFeature(coordinates, GeoFeature.GeoFeatureType.Residential);
                        baseShape = geoFeature;
                        shapes.Enqueue(geoFeature, geoFeature.ZIndex);
                    }
                    break;

                default:
                    break;
            }
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
