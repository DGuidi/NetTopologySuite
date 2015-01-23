using System;
using System.IO;
using System.Text;

namespace NetTopologySuite.IO
{
    public partial class DbaseFileReader
    {
        private readonly Encoding _encoding;

        private partial class DbaseFileEnumerator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbaseFileEnumerator"/> class.
            /// </summary>
            /// <param name="parent"></param>
            public DbaseFileEnumerator(DbaseFileReader parent) : this(parent._encoding)
            {
                if (parent == null) 
                    throw new ArgumentNullException("parent");

                _parent = parent;
                FileStream stream = new FileStream(parent._filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                _dbfStream = new BinaryReader(stream, _encoding);
                ReadHeader();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _dbfStream.Close();
            }
        }

        /// <summary>
        /// Initializes a new instance of the DbaseFileReader class.
        /// </summary>
        public DbaseFileReader(string filename, Encoding encoding)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            // check for the file existing here, otherwise we will not get an error
            //until we read the first record or read the header.
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
            }
            _filename = filename;
            _encoding = encoding;
        }


        /// <summary>
        /// Gets the header information for the dbase file.
        /// </summary>
        /// <returns>DbaseFileHeader contain header and field information.</returns>
        public DbaseFileHeader GetHeader()
        {
            if (_header == null)
            {
                FileStream stream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
                BinaryReader dbfStream = new BinaryReader(stream);

                _header = new DbaseFileHeader(_encoding);
                // read the header
                _header.ReadHeader(dbfStream, _filename);

                dbfStream.Close();
                stream.Close();

            }
            return _header;
        }
    }
}
