using System;
using System.Text.RegularExpressions;

namespace vcrossing.Code.Helpers;

public static class Resources
{

    private static List<string> FilePaths( string path, string pattern = ".*" )
    {
        var files = new List<string>();
        var dir = DirAccess.Open( path );
        dir.ListDirBegin();
        while ( true )
        {
            var file = dir.GetNext();

            // break out of loop if no more files
            if ( file == "" )
            {
                break;
            }

            var fullPath = $"{path}/{file}";
            fullPath = fullPath.Replace( ".import", "" ).Replace( ".remap", "" );

            // recursively search directories
            if ( dir.CurrentIsDir() )
            {
                files.AddRange( FilePaths( fullPath, pattern ) );
            }
            else
            {
                // skip files that don't match pattern
                if ( Regex.IsMatch( file, pattern ) == false )
                {
                    // Logger.Info( $"Skipping {fullPath}, didn't match {pattern}" );
                    continue;
                }

                // Logger.Info( $"Adding {fullPath}, matched {pattern}" );
                files.Add( fullPath );
            }
        }

        dir.ListDirEnd();
        return files;
    }

    public static List<string> GetFiles( string path, string pattern = ".*" )
    {
        return FilePaths( path, pattern );
    }

    public static List<Resource> LoadAllResources( string basePath, string pattern = ".*\\.tres" )
    {
        var paths = FilePaths( basePath, pattern );
        var resources = new List<Resource>();
        foreach ( var path in paths )
        {
            var resource = ResourceLoader.Load( path );
            resources.Add( resource );
        }
        return resources;
    }

}
