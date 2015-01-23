using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// A simple test class for write a complete (shp, shx and dbf) shapefile structure.
    /// </summary>
    public class ShapefileDataWriter
    {
        /// <summary>
        /// Creates a stub header.
        /// </summary>
        /// <param name="feature">The <see cref="IFeature"/> to use as schema.</param>
        /// <param name="count">Number of features to write.</param>
        /// <returns>The <see cref="DbaseFileHeader"/>.</returns>
        public static DbaseFileHeader GetHeader(IFeature feature, int count)
        {
            return GetHeader(feature, count, Shapefile.DefaultEncoding);
        }

        /// <summary>
        /// Creates a stub header.
        /// </summary>
        /// <param name="feature">The <see cref="IFeature"/> to use as schema.</param>
        /// <param name="count">Number of features to write.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to use.</param>
        /// <returns>The <see cref="DbaseFileHeader"/>.</returns>
        public static DbaseFileHeader GetHeader(IFeature feature, int count, Encoding encoding)
        {
            if (feature == null) 
                throw new ArgumentNullException("feature");
            if (encoding == null) 
                throw new ArgumentNullException("encoding");

            IAttributesTable attribs = feature.Attributes;
            string[] names = attribs.GetNames();
            DbaseFileHeader header = new DbaseFileHeader(encoding);
            header.NumRecords = count;
            foreach (string name in names)
            {
                Type type = attribs.GetType(name);
                if (type == typeof(double) || type == typeof(float))
                    header.AddColumn(name, 'N', DoubleLength, DoubleDecimals);
                else if (type == typeof(short) || type == typeof(ushort) ||
                         type == typeof(int) || type == typeof(uint) ||
                         type == typeof(long) || type == typeof(ulong))
                    header.AddColumn(name, 'N', IntLength, IntDecimals);
                else if (type == typeof(string))
                    header.AddColumn(name, 'C', StringLength, StringDecimals);
                else if (type == typeof(bool))
                    header.AddColumn(name, 'L', BoolLength, BoolDecimals);
                else if (type == typeof(DateTime))
                    header.AddColumn(name, 'D', DateLength, DateDecimals);
                else throw new ArgumentException("Type " + type.Name + " not supported");
            }
            return header;
        }

        /// <summary>
        /// Reads the header from a dbf file.
        /// </summary>
        /// <param name="dbfFile">The DBF file.</param>
        /// <returns>The <see cref="DbaseFileHeader"/>.</returns>
        public static DbaseFileHeader GetHeader(string dbfFile)
        {
            return GetHeader(dbfFile, Shapefile.DefaultEncoding);    
        }

        /// <summary>
        /// Reads the header from a dbf file.
        /// </summary>
        /// <param name="dbfFile">The DBF file.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to use.</param>
        /// <returns>The <see cref="DbaseFileHeader"/>.</returns>
        public static DbaseFileHeader GetHeader(string dbfFile, Encoding encoding)
        {            
            if (String.IsNullOrEmpty(dbfFile))
                throw new ArgumentNullException("dbfFile");
            if (encoding == null) 
                throw new ArgumentNullException("encoding");
            if (!File.Exists(dbfFile))
                throw new FileNotFoundException(dbfFile + " not found");

            DbaseFileHeader header = new DbaseFileHeader(encoding);
            header.ReadHeader(new BinaryReader(new FileStream(dbfFile, FileMode.Open, FileAccess.Read, FileShare.Read)), dbfFile);
            return header;
        }

        /// <summary>
        /// Creates an header from an array of <see cref="DbaseFieldDescriptor"/>s.
        /// </summary>
        /// <param name="dbFields">The <see cref="IFeature"/> to use as schema.</param>
        /// <param name="count">Number of features to write.</param>
        /// <returns>the <see cref="DbaseFileHeader"/>.</returns>
        public static DbaseFileHeader GetHeader(DbaseFieldDescriptor[] dbFields, int count)
        {
            return GetHeader(dbFields, count, Shapefile.DefaultEncoding);    
        }

        /// <summary>
        /// Creates an header from an array of <see cref="DbaseFieldDescriptor"/>s.
        /// </summary>
        /// <param name="dbFields">The <see cref="IFeature"/> to use as schema.</param>
        /// <param name="count">Number of features to write.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to use.</param>
        /// <returns>the <see cref="DbaseFileHeader"/>.</returns>
        public static DbaseFileHeader GetHeader(DbaseFieldDescriptor[] dbFields, int count, Encoding encoding)
        {
            if (dbFields == null) 
                throw new ArgumentNullException("dbFields");
            if (encoding == null) 
                throw new ArgumentNullException("encoding");

            DbaseFileHeader header = new DbaseFileHeader(encoding);
            header.NumRecords = count;
            foreach (DbaseFieldDescriptor dbField in dbFields)
                header.AddColumn(dbField.Name, dbField.DbaseType, dbField.Length, dbField.DecimalCount);
            return header;
        }

        private const int DoubleLength = 18;
        private const int DoubleDecimals = 8;
        private const int IntLength = 10;
        private const int IntDecimals = 0;
        private const int StringLength = 254;
        private const int StringDecimals = 0;
        private const int BoolLength = 1;
        private const int BoolDecimals = 0;
        private const int DateLength = 8;
        private const int DateDecimals = 0;

        private readonly string _shpFile = String.Empty;
        private readonly string _dbfFile = String.Empty;

        private readonly DbaseFileWriter _dbaseWriter;

        private DbaseFileHeader _header;

        /// <summary>
        /// Gets or sets the header of the shapefile.
        /// </summary>
        /// <value>The header.</value>
        public DbaseFileHeader Header
        {
            get { return _header; }
            set { _header = value; }
        }

        private IGeometryFactory _geometryFactory;
        private readonly Encoding _encoding;

        /// <summary>
        /// Gets or sets the geometry factory.
        /// </summary>
        protected IGeometryFactory GeometryFactory
        {
            get { return _geometryFactory; }
            set { _geometryFactory = value; }
        }

        /// <summary>
        /// Gets the <see cref="Encoding"/>.
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapefileDataWriter"/> class.
        /// </summary>
        /// <param name="fileName">File path without any extension.</param>
        public ShapefileDataWriter(string fileName) : 
            this(fileName, Geometries.GeometryFactory.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapefileDataWriter"/> class.
        /// </summary>
        /// <param name="fileName">File path without any extension.</param>
        /// <param name="factory">The <see cref="IGeometryFactory"/> to use.</param>
        public ShapefileDataWriter(string fileName, IGeometryFactory factory) :
            this(fileName, factory, Shapefile.DefaultEncoding) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapefileDataWriter"/> class.
        /// </summary>
        /// <param name="fileName">File path without any extension.</param>
        /// <param name="factory">The <see cref="IGeometryFactory"/> to use.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to use.</param>
        public ShapefileDataWriter(string fileName, IGeometryFactory factory, Encoding encoding)
        {
            if (factory == null) 
                throw new ArgumentNullException("factory");
            if (encoding == null) 
                throw new ArgumentNullException("encoding");

            _geometryFactory = factory;
            _encoding = encoding;

            // Files            
            _shpFile = fileName;
            _dbfFile = fileName + ".dbf";

            // Writers
            _dbaseWriter = new DbaseFileWriter(_dbfFile, _encoding);
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        public void Write(IList<IFeature> featureCollection)
        {
            // Test if the Header is initialized
            if (Header == null)
                throw new ApplicationException("Header must be set first!");
            
#if DEBUG
            // Test if all elements of the collections are features
            foreach (object obj in featureCollection)
                if (obj.GetType().IsAssignableFrom(typeof(IFeature)))
                    throw new ArgumentException("All the elements in the given collection must be " + typeof(IFeature).Name);
#endif

            try
            {
                // Write shp and shx  
                var geometries = new IGeometry[featureCollection.Count];
                var index = 0;
                foreach (IFeature feature in featureCollection)
                    geometries[index++] = feature.Geometry;
                ShapefileWriter.WriteGeometryCollection(_shpFile, new GeometryCollection(geometries, _geometryFactory));

                // Write dbf
                _dbaseWriter.Write(Header);
                foreach (IFeature feature in featureCollection)
                {
                    var attribs = feature.Attributes;
                    ArrayList values = new ArrayList();
                    for (int i = 0; i < Header.NumFields; i++)
                        values.Add(attribs[Header.Fields[i].Name]);
                    _dbaseWriter.Write(values);
                }
            }
            finally
            {
                // Close dbf writer
                _dbaseWriter.Close();
            }
        }
    }
}
