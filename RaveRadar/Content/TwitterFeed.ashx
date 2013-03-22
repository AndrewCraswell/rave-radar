<%@ WebHandler Language="C#" Class="CCssHttp" %>

/* Tell the CCss class that it is dealing with an HTTP context case */
#define HTTP_CONTEXT

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;


public class CCssHttp : IHttpHandler
{
	public bool IsReusable { get { return false; } }
	
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "text/css";
		
		/* Set up the conditional-css object */
		CCss oCcss = new CCss();
		oCcss.hcContext = context;
		oCcss.sUserAgent = context.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
		
		/* Run conditional css! */
		oCcss.fnComplete( new string[] {} );
	}
}

public class CCss
{
	public string[] asCssAlwaysFiles = new string[] {
	  "twitterFeed.css"
	};
	
	
	
	/*
	 * Variable: array object:aoGroups
	 * Purpose:  Browser group definitions
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public CCssGroup[] aoGroups = new CCssGroup[] {
		new CCssGroup( "cssA", "IE",     1, 6 ),     /* IE 6 and up */
		new CCssGroup( "cssA", "Gecko",  1, 1.0 ),   /* Mozilla 1.0 and up */
		new CCssGroup( "cssA", "Webkit", 1, 312 ),   /* Safari 1.3 and up  */
		new CCssGroup( "cssA", "SafMob", 1, 312 ),   /* All Mobile Safari  */
		new CCssGroup( "cssA", "Opera",  1, 7 ),     /* Opera 7 and up */
		new CCssGroup( "cssA", "Konq",   1, 3.3 ),   /* Konqueror 3.3 and up */
		new CCssGroup( "cssX", "IE",     0, 4 ),     /* IE 4 and down */
		new CCssGroup( "cssX", "IEMac",  0, 4.5 )    /* IE Mac 4 and down */
	};
	
	
	/*
	 * Variable: string:sVersion
	 * Purpose:  version information {major.minor.language.bugfix}
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public string sVersion = "1.2.cs_ashx.2";
	
	
	/*
	 * Variable: string:sUserBrowser
	 * Purpose:  Store the target browser
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public string sUserBrowser = "";
	
	
	/*
	 * Variable: double:dUserVersion
	 * Purpose:  Store the target browser version
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public double dUserVersion = 0;
	
	
	/*
	 * Variable: string:sUserGroup
	 * Purpose:  Store the target group
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public string sUserGroup = "";
	
	
	/*
	 * Variable: string:sUserAgent
	 * Purpose:  Store the target user agent string
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public string sUserAgent = "";
	
	
	/*
	 * Variable: string:sAuthor
	 * Purpose:  Author information for the CSS file for inclusion in the header
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public string sAuthor = "Andrew Craswell"; // "";
	
	
	/*
	 * Variable: string:sCopyright
	 * Purpose:  Copyright information for the CSS file for inclusion in the header
	 * Scope:    ConditionalCSS.CCss - public
	 */
	public string sCopyright = "2012"; // "";
	
	
	/*
	 * Variable: HttpContext:hcContext
	 * Purpose:  HTTP context for ASHX version
	 * Scope:    ConditionalCSS.CCss - public
	 */
	#if HTTP_CONTEXT
		public HttpContext hcContext = null;
	#else
		public string hcContext = null;
	#endif
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private variables
	 */
	
	/*
	 * Variable: array string:_asCSSFiles
	 * Purpose:  CSS files to be read
	 * Scope:    ConditionalCSS.CCss - private
	 */
	private List<string> _tsCSSFiles = new List<string>();
	
	
	/*
	 * Function: string:sCSS
	 * Purpose:  css buffer
	 * Scope:    ConditionalCSS.CCss - private
	 */
	private string _sCSS = "";
	
	
	/*
	 * Variable: float:_iBrowserPrecision
	 * Purpose:  Error allowance for precision
	 * Scope:    ConditionalCSS.CCss - private
	 */
	private int _iBrowserPrecision = 10;
	
	
	
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public methods
	 */
	
	/*
	 * Function: fnComplete
	 * Purpose:  Perform a standard Conditional-CSS parsing run on input files
	 * Returns:  -
	 * Inputs:   -
	 * Notes:    You might want to customise this function or use the public functions to create
	 *   your own processing functionality
	 */
	public void fnComplete ( string[] asArgs )
	{
		/*
		 * Set up the required variables based on input
		 */
		this.fnSetUserBrowserGET();   /* Allow GET vars */
		int iOptind = this.fnSwitches( asArgs );  /* CLI switches */
		this.fnSetUserBrowser();
		this.fnSetBrowserGroup();
	
		this.fnOutputHeader();
		
		/*
		 * Add files
		 */
		for ( int i=iOptind ; i<asArgs.Length ; i++ )
		{
			this.fnAddFile( asArgs[i] );
		}
		
		for ( int i=0 ; i<asCssAlwaysFiles.Length ; i++ )
		{
			this.fnAddFile( this.asCssAlwaysFiles[i] );
		}
		
		
		/*
		 * Read all required files
		 */
		this.fnReadCSSFiles();
		this.fnCssIncludes();
	
		/*
		 * Do the c-css magic on the imported files
		 */
		this.fnStripComments();
		this.fnProcess();
		this.fnOutput();
	}
	
	
	/*
	 * Function: fnProcess
	 * Purpose:  Strip multi-line comments from the target css
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnProcess ()
	{
		/* Regex defs */
		Regex regexBlock = new Regex( ".*?{((?>[^{}]*))*}", RegexOptions.Singleline );
		MatchCollection matchesBlock = regexBlock.Matches( this._sCSS );
		
		Regex regexCCMatchBlock = new Regex( "(\\[if .*?\\]).*?{", RegexOptions.Singleline );
		Regex regexCCMatch = new Regex( "(\\[if .*?\\])", RegexOptions.Singleline );
		
		Regex regexRemoveStatement = new Regex( "\\[if .*?\\].*?(;|})", RegexOptions.Singleline );
		Regex regexEndBlock = new Regex( "}.*?$", RegexOptions.Singleline );
		
		
		/* Break the CSS down into blocks */
		for ( int i=0 ; i<matchesBlock.Count ; i++ )
		{
			int iProcessBlock = 1;
			string sBlockOriginal = matchesBlock[i].Value;
			string sBlockModified = sBlockOriginal;
			
			/* Find if the block has a conditional comment */
			if ( regexCCMatchBlock.IsMatch( sBlockOriginal ) )
			{
				string sCondition = regexCCMatchBlock.Match( sBlockOriginal ).Groups[1].Value;
				
				/* Find out if the block should be included or not */
				if ( this._fnCheckCC( sCondition ) == false )
				{
					iProcessBlock = 0;
					
					/* Drop the block from the output string */
					sBlockModified = "";
				}
				else
				{
					/* Include the block and remove the conditional comment */
					sBlockModified = sBlockOriginal.Replace( sCondition, "" );
				}
			}
			
			/* If the block should be processed */
			if ( iProcessBlock == 1 )
			{
				/* Loop over the block looking for conditional comment statements */
				while ( regexCCMatch.IsMatch( sBlockModified ) )
				{
					string sCondition = regexCCMatch.Match( sBlockModified ).Groups[1].Value;
					
					/* See if statement should be included or not */
					if ( this._fnCheckCC( sCondition ) == false )
					{
						/* Remove statement - note that this might remove the trailing
						 * } of the block! This is valid css as the last statement is
						 * implicitly closed by the }. So we moke sure there is one at the
						 * end later on
						 */
						string sExpression = regexRemoveStatement.Match( sBlockModified ).Value;
						sBlockModified = sBlockModified.Replace( sExpression, "" );
					}
					else
					{
						/* Remove CC */
						sBlockModified = sBlockModified.Replace( sCondition, "" );
					}
				}
			}
			
			/* Ensure the block has a closing */
			if ( sBlockModified != "" && !regexEndBlock.IsMatch( sBlockModified ) )
			{
				sBlockModified += "}";
			}
			
			/* Swap in the modifed block */
			if ( sBlockOriginal != sBlockModified )
			{
				this._sCSS = this._sCSS.Replace( sBlockOriginal, sBlockModified );
			}
		}
	}
	
	
	/*
	 * Function: fnOutput
	 * Purpose:  Remove extra white space and output
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnOutput ()
	{
		/* Remove the white space in the css - while preserving the needed spaces */
		Regex regexWhiteSpace = new Regex( "\\s", RegexOptions.Singleline );
		this._sCSS = regexWhiteSpace.Replace( this._sCSS, " " );
		
		while ( this._sCSS.IndexOf( "  " ) != -1 )
		{
			this._sCSS = this._sCSS.Replace( "  ", " " );
		}
		
		/* Add new lines for basic legibility */
		this._sCSS = this._sCSS.Replace( "} ", "}\n" );
		
		/* Phew - we finally got there... */
		this._fnWrite( this._sCSS.Trim() );
	}
	
	
	/*
	 * Function: fnStripComments
	 * Purpose:  Strip multi-line comments from the target css
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnStripComments ()
	{
		Regex regexComment = new Regex( "/\\*.*?\\*/", RegexOptions.Singleline );
		this._sCSS = regexComment.Replace( this._sCSS, "" );
	}
	
	
	/*
	 * Function: fnCssIncludes
	 * Purpose:  Check the input for @import statements and include files found
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnCssIncludes ()
	{
		// First remove any comments as they could get in the way
		this.fnStripComments();
		
		// Find all conditional @import statements
		Regex regexConditionalImport = new Regex( "\\[if .*?\\]\\s*?@import .*?;", 
			RegexOptions.Singleline );
		while ( regexConditionalImport.IsMatch( this._sCSS ) )
		{
			string sFullStatement = regexConditionalImport.Match( this._sCSS ).Value;
			
			Regex regexCondition = new Regex( "\\[if .*?\\]" );
			string sCondition = regexCondition.Match( sFullStatement ).Value;
			
			string sImport = sFullStatement.Replace( sCondition, "" );
			sImport = sImport.Trim();
			
			this.fnCssImport( sImport, this._fnCheckCC(sCondition), sFullStatement );
		}
		
		// Findall non-conditional @import statements
		Regex regexImport = new Regex( "@import .*?;" );
		while ( regexImport.IsMatch( this._sCSS ) )
		{
			this.fnCssImport( regexImport.Match( this._sCSS ).Value, true, 
				regexImport.Match( this._sCSS ).Value );
		}
	}


	/*
	 * Function: fnCssImport
	 * Purpose:  Deal with an import CSS file
	 * Returns:  -
	 * Inputs:   string:sImportStatement - @import...
	 *           bool:bImport - include the file or not
	 *           string:sFullImport - The full string to remove
	 */
	public void fnCssImport ( string sImportStatement, bool bImport, string sFullImport )
	{
		string sCssFile;
		string sImportedCSS;
		
		if ( bImport )
		{
			/* Parse @import to get the URL */
			sCssFile = this._fnParseImport( sImportStatement );
			
			/* Read the CSS file */
			sImportedCSS = this._fnReadCSSFile( sCssFile );
			
			/* Save it back into the main css string */
			this._sCSS = this._sCSS.Replace( sFullImport, sImportedCSS );
		}
		else
		{
			/* Remove the import statement */
			this._sCSS = this._sCSS.Replace( sFullImport, "" );
		}
	}
	
	
	/*
	 * Function: fnReadCSSFiles
	 * Purpose:  Read the CSS files
	 * Returns:  -
	 * Inputs:   string:... - any number of string variables pointing to files
	 */
	public void fnReadCSSFiles ()
	{
		this._sCSS = "";
		
		for ( int i=0 ; i<this._tsCSSFiles.Count ; i++ )
		{
			this._sCSS += this._fnReadCSSFile( this._tsCSSFiles[i] );
		}
	}
	
	
	/*
	 * Function: fnOutputHeader
	 * Purpose:  Header output with information
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnOutputHeader ()
	{
		// Add comment to output
		this._fnWrite(
			 "/*\n"
			+" * c-css by U4EA Technologies - Allan Jardine\n"
			+" * Version:       "+ this.sVersion +"\n" );
		
		if ( this.sAuthor != "" )
		{
			this._fnWrite(
			 " * CSS Author:    "+ this.sAuthor +"\n" );
		}
		
		if ( this.sCopyright != "" )
		{
			this._fnWrite(
			 " * Copyright:     "+ this.sCopyright +"\n" );
		}
		
		this._fnWrite(
			 " * Browser:       "+ this.sUserBrowser +" "+ this.dUserVersion +"\n"
			+" * Browser group: "+ this.sUserGroup +"\n"
			+" */\n" );
		
		/* X grade CSS means the browser doesn't see the CSS at all */
		if ( this.sUserGroup == "cssX" )
		{
			System.Environment.Exit( 0 );
		}
	}
	
	
	/*
	 * Function: fnSetBrowserGroup
	 * Purpose:  Based on the browser grouping we set a short hand method for 
	 *   access
	 * Returns:  void
	 * Inputs:   array:aGroups - group information
	 */
	public void fnSetBrowserGroup ( )
	{
		for ( int i=0 ; i<aoGroups.Length ; i++ )
		{
			if ( aoGroups[i].sEngine == this.sUserBrowser )
			{
				if ( aoGroups[i].iGreaterOrEqual == 1 &&
				     aoGroups[i].dVersion <= this.dUserVersion )
				{
					this.sUserGroup = aoGroups[i].sGrade;
					break;
				}
				else if ( aoGroups[i].iGreaterOrEqual == 0 &&
				          aoGroups[i].dVersion >= this.dUserVersion )
				{
					this.sUserGroup = aoGroups[i].sGrade;
					break;
				}
			}
		}
	}
	
	
	/*
	 * Function: fnSwitches
	 * Purpose:  Deal with command line switches
	 * Returns:  int:i - Where the sitches end in argc/v
	 * Inputs:   -
	 * Notes:    Oddly enough c# doesn't have a command line switch processer like
	 *   getopt in c. There are libraries available, but I believe this is good 
	 *   enough for the moment here!
	 */
	public int fnSwitches ( string[] asArgs )
	{
		for ( int i=0 ; i<asArgs.Length ; i++ )
		{
			if ( asArgs[i][0] == '-' || asArgs[i][0] == '/' )
			{
				if ( asArgs[i][1] == 'b' )
				{
					i++;
					this.sUserBrowser = asArgs[i];
				}
				else if ( asArgs[i][1] == 'v' )
				{
					i++;
					this.dUserVersion = Convert.ToDouble( asArgs[i] );
				}
				else if ( asArgs[i][1] == 'u' )
				{
					i++;
					this.sUserAgent = asArgs[i];
				}
				else if ( asArgs[i][1] == 'a' )
				{
					i++;
					this.sAuthor = asArgs[i];
				}
				else if ( asArgs[i][1] == 'c' )
				{
					i++;
					this.sCopyright = asArgs[i];
				}
				else if ( asArgs[i][1] == 'h' )
				{
					this.fnOutputUsage();
					System.Environment.Exit( 0 );
				}
				else if ( asArgs[i][1] == 'i' )
				{
					/* Give a CSS MIME type so the browser knows this is a css file */
					this._fnWrite( "Content-type: text/css\n\n" );
				}
			}
			else
			{
				return i;
			}
		}
		
		return 1;
	}
	
	
	/*
	 * Function: fnOutputUsage()
	 * Purpose:  Output the usage to the user
	 * Returns:  void
	 * Inputs:   void
	 */
	public void fnOutputUsage(  )
	{
	  this._fnWrite(
	     "Usage: c-css.exe [OPTIONS]... [FILE]...\n"
	    +"Parse a CSS file which contains IE style conditional comments into a\n"
	    +"stylesheet which is specifically suited for a particular web-browser.\n"
	    +"\n"
	    +" -a (or /a)   Set the stylesheet's author name for the information header.\n"
	    +" -b (or /b)   Use this particular browser. Requires that the \n"
	    +"              browser version must also be set, -v. Options are:\n"
	    +"                IE\n"
	    +"                IEMac\n"
	    +"                Gecko\n"
	    +"                Webkit\n"
	    +"                Opera\n"
	    +"                Konq\n"
	    +"                NetF\n"
	    +"                PSP\n"
	    +" -c (or /c)   Set the copyright header for the information header.\n"
	    +" -h (or /h)   This help information.\n"
	    +" -u (or /u)   Browser user agent string.\n"
  		+" -i (or /i)   Print the CSS content type (text/css).\n"
	    +" -v (or /v)   Use this particular browser version. Requires that\n"
	    +"                the browser must also be set using -b.\n"
	    +"\n"
	    +"The resulting stylesheet will be printed to stdout.\n"
	    +"\n"
	    +"Example usage:\n"
	    +" c-css.exe -b IE -v 6 example.css\n"
	    +"        Parse a style sheet for Internet Explorer v6\n"
	    +"\n"
	    +" c-css.exe -b Webkit -v 897 demo1.css demo2.css\n"
	    +"        Parse two style sheets for Webkit (Safari) v897\n"
	    +"\n"
	    +" c-css.exe -u \"Mozilla/4.0 (compatible; MSIE 5.5;)\" example.css\n"
	    +"        Parse stylesheet for the specified user agent string\n"
	    +"\n"
	    +"Report bugs to <software@sprymedia.co.uk>\n"
	    +"\n"
	  );
	}
	
	
	/*
	 * Function: fnMatchUserAgent
	 * Purpose:  Set the user's browser information
	 * Returns:  int: - 1 for no match, 0 for matched
	 * Inputs:   string:sMatch - the regular expression string to use for matching
	 *           string:sBrowser - name to give the browser if match occurs
	 */
	public int fnMatchUserAgent( string sMatch, string sBrowser )
	{
		string[] asMatches;
		Regex oBrowserExp = new Regex( sMatch, RegexOptions.IgnoreCase );
		
		if ( oBrowserExp.IsMatch( this.sUserAgent ) )
		{
			asMatches = oBrowserExp.Split( this.sUserAgent );
			this.sUserBrowser = sBrowser;
			string sLocalUserVersion = asMatches[1];
			
			/* Round the version number to one decimal place */
			int iDot = sLocalUserVersion.IndexOf( "." );
			if ( iDot > 0 )
			{
				this.dUserVersion = Convert.ToDouble( sLocalUserVersion.Substring( 0, iDot+2 ) );
			}
			
			return 0;
		}
		
		return 1;
	}
	
	
	/*
	 * Function: fnSetUserBrowser
	 * Purpose:  Set the user's browser information
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnSetUserBrowser ()
	{
		/* Check we are not overriding a CLI or GET set of the browser */
		if ( this.sUserBrowser != "" )
		{
			return;
		}
		
		if ( this.sUserAgent == "" )
		{
			string sLocalUserAgent = System.Environment.GetEnvironmentVariable("HTTP_USER_AGENT");
			if ( sLocalUserAgent == null )
			{
				return;
			}
			this.sUserAgent = sLocalUserAgent;
		}
		
		// Safari Mobile
		if ( this.fnMatchUserAgent( @"mozilla.*applewebkit/([0-9a-z+-.]+).*mobile.*", "SafMob" ) == 0 )
		{
			return;
		}
		
		// Webkit (Safari, Shiira etc)
		if ( this.fnMatchUserAgent( @"mozilla.*applewebkit/([0-9a-z+-.]+).*", "Webkit" ) == 0 )
		{
			return;
		}
		
		// Opera
		if ( this.fnMatchUserAgent( @"mozilla.*opera ([0-9a-z+-.]+).*", "Opera" ) == 0 ||
		     this.fnMatchUserAgent( @"^opera/([0-9a-z+-.]+).*", "Opera" ) == 0 )
		{
			return;
		}
		
		// Gecko (Firefox, Mozilla, Camino etc)
		if ( this.fnMatchUserAgent( @"mozilla.*rv:([0-9a-z+-.]+).*gecko.*", "Gecko" ) == 0 )
		{
			return;
		}
		
	  // IE Mac
		if( this.fnMatchUserAgent( @"/mozilla.*MSIE ([0-9a-z+-.]+).*Mac.*", "IEMac" ) == 0 )
		{
			return;
		}
		
	  // MS mobile
		if( this.fnMatchUserAgent( @"PPC.*IEMobile ([0-9a-z+-.]+).*", "IEMob" ) == 0 )
		{
			return;
		}
		
		// MSIE
		if( this.fnMatchUserAgent( @"mozilla.*?MSIE ([0-9a-z+-.]+).*", "IE" ) == 0 )
		{
			return;
		}
		
		// Konqueror
		if( this.fnMatchUserAgent( @"mozilla.*konqueror/([0-9a-z+-.]+).*", "Konq" ) == 0 )
		{
			return;
		}
		
		// PSP
		if( this.fnMatchUserAgent( @"mozilla.*PSP.*; ([0-9a-z+-.]+).*", "PSP" ) == 0 )
		{
			return;
		}
		
		// NetFront
		if( this.fnMatchUserAgent( @"mozilla.*NetFront/([0-9a-z+-.]+).*", "NetF" ) == 0 )
		{
			return;
		}
	}
	
	
	/*
	 * Function: fnSetUserBrowserGET
	 * Purpose:  Set the user's browser information based on GET vars is there
	 *   are any
	 * Returns:  -
	 * Inputs:   -
	 */
	public void fnSetUserBrowserGET ()
	{
		string sValue;
		
		sValue = this.fnGetVariable( "b" );
		if ( sValue != null )
		{
			this.sUserBrowser = sValue;
		}
		
		sValue = this.fnGetVariable( "browser" );
		if ( sValue != null )
		{
			this.sUserBrowser = sValue;
		}
		
		sValue = this.fnGetVariable( "v" );
		if ( sValue != null )
		{
			this.dUserVersion = Convert.ToDouble( sValue );
		}
		
		sValue = this.fnGetVariable( "version" );
		if ( sValue != null )
		{
			this.dUserVersion = Convert.ToDouble( sValue );
		}
		
		sValue = this.fnGetVariable( "a" );
		if ( sValue != null )
		{
			this.sAuthor = sValue;
		}
		
		sValue = this.fnGetVariable( "author" );
		if ( sValue != null )
		{
			this.sAuthor = sValue;
		}
		
		sValue = this.fnGetVariable( "c" );
		if ( sValue != null )
		{
			this.sCopyright = sValue;
		}
		
		sValue = this.fnGetVariable( "copyright" );
		if ( sValue != null )
		{
			this.sCopyright = sValue;
		}
	}
	
	
	/*
	 * Function: fnGetVariable
	 * Purpose:  Get a specific variable from an HTTP request string
	 * Returns:  string: found string or null
	 * Inputs:   string:sVar - variable to get
	 */
	public string fnGetVariable ( string sVar )
	{
		string sQuery = System.Environment.GetEnvironmentVariable("QUERY_STRING");
		if ( sQuery == null )
		{
			return null;
		}
		
		string sVarEquals = sVar + "=";
		string[] asSplit = sQuery.Split( '&' );
		for ( int i=0 ; i<asSplit.Length ; i++ )
		{
			if ( asSplit[i].StartsWith( sVarEquals ) )
			{
				return asSplit[i].Substring( sVar.Length+1 );
			}
		}
		
		return null;
	}
	
	
	/*
	 * Function: fnAddFile
	 * Purpose:  add new file to be processed
	 * Returns:  -
	 * Inputs:   string:... - string pointing to files
	 */
	public void fnAddFile ( string sFiles )
	{
		this._tsCSSFiles.Add( sFiles );
	}
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private methods
	 */
	
	private void _fnWrite ( string sOutput )
	{
		#if HTTP_CONTEXT
			this.hcContext.Response.Write( sOutput );
		#else
			Console.Write( sOutput );
		#endif
	}
	
	
	/*
	 * Function: _fnParseImport
	 * Purpose:  Get the import URI from the import statement
	 * Returns:  string:  - Import URL
	 * Inputs:   string:sImport - @import CSS statement
	 */
	private string _fnParseImport ( string sImport )
	{
		string[] aImport = sImport.Split( ' ' );
		
		string sUrl = aImport[1].Trim();
		
		if ( sUrl.StartsWith( "url" ) )
		{
			sUrl = sUrl.Substring( 3 );
		}
		
		sUrl = sUrl.Replace( "(", "" );
		sUrl = sUrl.Replace( "'", "" );
		sUrl = sUrl.Replace( "\"", "" );
		sUrl = sUrl.Replace( ";", "" );
		return sUrl;
	}
	
			
	/*
	 * Function: _fnReadCSSFile
	 * Purpose:  Read a CSS file
	 * Returns:  string:sCSS - the contents of the css file
	 * Inputs:   string:sPath - the file name and path to be read
	 */
	private string _fnReadCSSFile ( string sPath )
	{
		/* On a web-server we need to translate the paths */
		#if HTTP_CONTEXT
			sPath = this.hcContext.Server.MapPath( sPath );
		#endif
		
		if ( System.IO.File.Exists( sPath ) )
		{
			/* Read the CSS file */
			string sCss = System.IO.File.ReadAllText( sPath );
			
			/* If there is a hash-bang line - strip it out for compatability with C */
			Regex regexHashBang = new Regex( "^(#!.*?\n)" );
			sCss = regexHashBang.Replace( sCss, "" );
			
			return sCss;
		}
		else
		{
			this._fnWrite( "/*** Warning: The file "+sPath+" could not be found ***/\n" );
			return "";
		}
	}
	
	
	/*
	 * Function: _fnCheckCC
	 * Purpose:  See if a conditional comment should be processed
	 * Returns:  bool: true-process, false-don't process
	 * Inputs:   string:sCC - the conditional comment
	 *
	 * Notes:
	 * The browser conditions are:
	 *  [if {!} {browser}]
	 *  [if {!} {browser} {version}]
	 *  [if {!} {condition} {browser} {version}]
	 */
	private bool _fnCheckCC ( string sCondition )
	{
		/* Strip brackets from the CC */
		sCondition = sCondition.Replace( "[", "" );
		sCondition = sCondition.Replace( "]", "" );
		List<string> listsCondition = new List<string>( sCondition.Split( ' ' ) );
		
		bool bNegate = false;
		bool bInclude = false;
		
		if ( listsCondition[1] == "!" )
		{
			bNegate = true;
			
			/* Remove the negation operator so all the other operators are in place */
			listsCondition.RemoveAt( 1 );
		}
		
		/* Do the logic checking
		 * If the CC is an integer, then we drop the minor version number from the
		 * users browser. This means that if the user is using v5.5, and the
		 * statement is for v5, then it matches. To stop this a CC with v5.0 would
		 * have to be used
		 */
		double dLocalUserVersion = this.dUserVersion;
		if ( listsCondition.Count == 3 && listsCondition[2].IndexOf( '.' ) == -1 )
		{
			int iTmpVersion = (int)dLocalUserVersion;
			dLocalUserVersion = (double)iTmpVersion;
		}
		else if ( listsCondition.Count == 4 && listsCondition[3].IndexOf( '.' ) == -1 )
		{
			int iTmpVersion = (int)dLocalUserVersion;
			dLocalUserVersion = (double)iTmpVersion;
		}
		
		/* All comparisions are done using ints */
		int iLocalUserVersion = Convert.ToInt32( dLocalUserVersion * _iBrowserPrecision );
		
		/* Just the browser */
		if ( listsCondition.Count == 2 )
		{
			if ( this.sUserBrowser == listsCondition[1] ||
				   this.sUserGroup == listsCondition[1] )
			{
				bInclude = true;
			}
		}
		/* Browser and version */
		else if ( listsCondition.Count == 3 )
		{
			int iConditionVersion = 
				Convert.ToInt32( Convert.ToSingle(listsCondition[2]) * _iBrowserPrecision );
			
			if ( this.sUserBrowser == listsCondition[1] && 
				   iLocalUserVersion == iConditionVersion )
			{
				bInclude = true;
			}
		}
		else if ( listsCondition.Count == 4 )
		{
			int iConditionVersion = 
				Convert.ToInt32( Convert.ToSingle(listsCondition[3]) * _iBrowserPrecision );
		
			if ( listsCondition[1] == "lt" )
			{
				if ( this.sUserBrowser == listsCondition[2] &&
				   iLocalUserVersion < iConditionVersion )
				{
					bInclude = true;
				}
			}
			else if ( listsCondition[1] == "lte" )
			{
				if ( this.sUserBrowser == listsCondition[2] &&
				   iLocalUserVersion <= iConditionVersion )
				{
					bInclude = true;
				}
			}
			else if ( listsCondition[1] == "eq" )
			{
				if ( this.sUserBrowser == listsCondition[2] &&
				   iLocalUserVersion == iConditionVersion )
				{
					bInclude = true;
				}
			}
			else if ( listsCondition[1] == "gte" )
			{
				if ( this.sUserBrowser == listsCondition[2] &&
				   iLocalUserVersion >= iConditionVersion )
				{
					bInclude = true;
				}
			}
			else if ( listsCondition[1] == "gt" )
			{
				if ( this.sUserBrowser == listsCondition[2] &&
				   iLocalUserVersion > iConditionVersion )
				{
					bInclude = true;
				}
			}
		}
		
		/* Perform negation if required */
		if ( bNegate )
		{
			bInclude = bInclude ? false : true;
		}
		
		/* Return the required type */
		return bInclude;
	}
}


/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Class:   CCssGroup
 * Purpose: CSS browser groups class
 * Notes:    Browsers can be groups together such that a  single conditional
 *   statement can refer to multiple browsers. For example 'cssA' might be 
 *   top level css support
 *           This is absolutely identical to the CCssGroup class found in c-cssGroup.class.cs but
 *   is included here for the simplity of creating the ashx page. If you change one - change the 
 *   other!!
 */
public class CCssGroup
{
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public variables
	 */
	
	/*
	 * Variable: sGrade
	 * Purpose:  Grade of the browser group
	 * Scope:    ConditionalCSS.CCssGroup
	 */
	public string sGrade;
	
	
	/*
	 * Variable: sEngine
	 * Purpose:  Name of the browser engine
	 * Scope:    ConditionalCSS.CCssGroup
	 */
	public string sEngine;
	
	
	/*
	 * Variable: iGreaterOrEqual
	 * Purpose:  Indicate if the rule applies when greater or equal to the version (1) or less
	 *   that (0).
	 * Scope:    ConditionalCSS.CCssGroup
	 */
	public int iGreaterOrEqual;
	
	
	/*
	 * Variable: dVersion
	 * Purpose:  Version number of the browser - used in combination with iGreaterOrEqual
	 * Scope:    ConditionalCSS.CCssGroup
	 */
	public double dVersion;
	
	
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public methods
	 */
	
	/*
	 * Function: CCssGroup - constructor
	 * Purpose:  Constructor for group object
	 * Returns:  -
	 * Inputs:   string:sGrade
	 *           string:sEngine
	 *           int:iGreaterOrEqual
	 *           double:dVersion
	 */
	public CCssGroup ( string sGrade, string sEngine, int iGreaterOrEqual, double dVersion )
	{
		this.sGrade = sGrade;
		this.sEngine = sEngine;
		this.iGreaterOrEqual = iGreaterOrEqual;
		this.dVersion = dVersion;
	}
}
