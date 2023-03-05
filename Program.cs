using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DirectoryWarmer
{
	internal class Program
	{
		static long totalBytesRead = 0;
		static long totalFilesRead = 0;
		static long totalCycles = 0;
		static bool pauseBetweenFiles = false;
		static int scanInterval = 3600;
		static ManualResetEvent mre = new ManualResetEvent( false );


		static void Main( string[] args )
		{
			string path = Environment.CurrentDirectory;

			if ( args.Length > 0 )

				path = args[0];


			Task.Run( () =>
			{
				while ( true )
				{
					Console.WriteLine( DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) + " Scanning " + path + "... " );
					Stopwatch w = Stopwatch.StartNew();
					var sr = ScanDir( path );
					w.Stop();
					var nextScan = DateTime.Now.AddSeconds( scanInterval );
					Console.WriteLine( DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ) + " Scan done in " + w.Elapsed + "  (" + sr.FilesRead + " files) nextScan=" + nextScan.ToString( "HH:mm:ss" ) + " " );
					mre.WaitOne( scanInterval * 1000 );

					totalBytesRead += sr.BytesRead;
					totalFilesRead += sr.FilesRead;
					totalCycles++;
				}


			} );






			Task.Run( () =>
			{
				while ( true )
				{
					//Console.WriteLine( totalFilesRead );

					mre.WaitOne( 1000 );
				}
			} );


			Console.ReadLine();
		}


		public class ScanResult
		{

			public long FilesRead { get; set; }
			public long BytesRead { get; set; }
		}


		public static ScanResult ScanDir( string path )
		{
			var scanResult = new ScanResult();
			ScanDir( new DirectoryInfo( path ), 0, scanResult, null );

			return scanResult;
		}

		static Random rng = new Random();
		static char[] rngAlphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
		static char[] rngAlphabetWithWhitespaces = "aaaaaaaaaaaaaaaaaaaaa \n".ToCharArray();

		public static void ScanDir( DirectoryInfo dir, int depth, ScanResult scanResult, string dummyPath )
		{
			string pad = "".PadLeft( depth * 3 );

			foreach ( var fi in dir.GetFiles() )
			{
				try
				{
					var fileContents = File.ReadAllBytes( fi.FullName );


					scanResult.BytesRead += fileContents.LongLength;
					scanResult.FilesRead++;

 

				}
				catch ( Exception ex )
				{

				}
				if ( pauseBetweenFiles )
					mre.WaitOne( 1 );
			}

			foreach ( var di in dir.GetDirectories() )
			{

				ScanDir( di, depth + 1, scanResult, null );
			}
		}
	}
}
