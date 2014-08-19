@if (@CodeSection == @Batch) @then

:: The first line above is...
:: - in Batch: a valid IF command that does nothing.
:: - in JScript: a conditional compilation IF statement that is false,
::               so this Batch section is omitted until next "arroba-end".


@echo off

rem FindRepl.bat: Utility to search and search/replace strings in a file
rem Antonio Perez Ayala 
rem   - June/26/2013: first version.
rem   - July/02/2013: use /Q in submatched substrings, use /VR for /V in Replace, eliminate "\r\n" in blocks.
rem   - July/07/2013: change /VR by /R, search for "\r\n" with /$ and no /V, /N nor blocks.

if "%~1" neq "" if "%~1" neq "/?" goto begin
   < "%~F0" CScript //nologo //E:JScript "%~F0" "^<usage>" /E:"^</usage>" /O:+1:-1
   goto :EOF
:begin
   CScript //nologo //E:JScript "%~F0" %*
   exit /B %errorlevel%
:end

<usage>
Searches for strings in a file, and prints or replaces them.

FINDREPL [/I] [/V] [/N] rSearch [/E:rEndBlk] [/O:s:e] [/B:rBlock] [/$:s1...]
         [[/R] [/A] sReplace] [/Q:c] [/S:sSource]

  /I         Specifies that the search is not to be case-sensitive.
  /V         Prints only lines that do not contain a match.
  /N         Prints the line number before each line that matches.
  rSearch    Text to be searched for, or Start text for a block of lines.
  /E:rEndBlk Text to be searched for the End of a block of lines.
  /O:s:e     Specifies offsets for Starting and Ending lines of blocks.
  /B:rBlock  Text to be searched again in the blocks of lines.
  /$:s1...   Specifies to print saved submatched substrings instead of lines.
  /R         Prints only replaced lines, instead of all file lines.
  /A         Specifies that sReplace has alternative values matching rSearch.
  sReplace   Text that will replace the matched text.
  /Q:c       Specifies a character that is used in place of quotation marks.
  /S:sSource Text to be processed instead of Stdin file.

All search texts must be given in VBScript regular expression format. See:
http://msdn.microsoft.com/en-us/library/ee236359(v=vs.84).aspx
(^ and $ anchors the beginning and end of line, respectively)
The replacement string may use $ to retrieve saved submatched substrings. See:
http://msdn.microsoft.com/en-us/library/t0kbytzc(v=vs.84).aspx
Use /A switch to insert several values separated by pipe in rSearch/sReplace.
Use /Q:c switch to insert a quote in the search/replacement texts.
If first character of any text is an equal-sign, specify a Batch variable.

There are three ways to define Blocks of lines using rSearch text as base:

/O:s:e                           /E:rEndBlk              /E:rEndBlk /O:s:e
-------------------------------------------------------------------------------
Add S and E (with optional       From rSearch line       Add S to rSearch line
signs) to matching lines.        to rEndBlk line.        and E to rEndBlk line.

... and one way more if rSearch is not given:   /O:s:e   From line S to line E.
If S or E is negative, specify a backwards line from the end of file.
If E is not given, then it defaults to the last line of the file (same as -1).

The output vary depending on the given parameters and switches this way:

rSearch        /V                 Block            /B:rBlock        /$:s1...
-------------------------------------------------------------------------------
Matched        Non-matched        Blocks of        Search /B:       Saved
lines.         lines.             lines.           in blocks        submatches.

sReplace       /R                 Block            /B:rBlock
-------------------------------------------------------------------------------
All file       Only replaced      Search /B:rBlock in blocks
lines.         file lines.        and replaces matched lines.

The total number of matchings/replacements is returned in ERRORLEVEL.
</usage>

End of Batch section


@end


// JScript section


// FINDREPL [/I] [/V] [/N] rSearch [/E:rEndBlk] [/O:s:e] [/B:rBlock] [/$:s1...]
//          [[/R] [/A] sReplace] [/Q:c] [/S:source]


var options = WScript.Arguments.Named,
    args    = WScript.Arguments.Unnamed,
    env     = WScript.CreateObject("WScript.Shell").Environment("Process"),

    ignoreCase   = options.Exists("I")?"i":"",
    notMatched   = options.Exists("V"),
    showNumber   = options.Exists("N"),
    search       = undefined,
    endBlk       = undefined,
    offset       = undefined,
    block        = undefined,
    submatches   = undefined,
    justReplaced = options.Exists("R"),
    replace      = undefined,
    quote        = options.Item("Q"),

    lineNumber = 0, range = new Array(),
    procLines = false, procBlocks = false,
    nextMatch, result  = 0,

    match = function ( line, regex ) { return line.search(regex) >= 0; },

    parseInts = 
       function ( strs ) { 
          var Ints = new Array();
          for ( var i = 0; i < strs.length; ++i ) {
             Ints[i] = parseInt(strs[i]); 
          }
          return Ints;
       },

    getRegExp =
       function ( param, justLoad ) {
          var result = param;
          if ( result.substr(0,1) == "=" ) {
             result = env(result.substr(1));
          } else {
             if ( quote != undefined ) result = result.replace(eval("/"+quote+"/g"),'"');
          }
          if ( ! justLoad ) result = new RegExp(result,"gm"+ignoreCase);
          return result;
       }
    ;


if ( args.Length > 0 ) {
   search = getRegExp(args.Item(0),true);
}
if ( options.Exists("E") ) {
   endBlk = getRegExp(options.Item("E"));
   procBlocks = true;
}
if ( options.Exists("O") ) {
   offset = parseInts(options.Item("O").split(":"));
   procBlocks = true;
}
if ( options.Exists("B") ) {
   block = getRegExp(options.Item("B"),true);
}
if ( options.Exists("$") ) submatches = parseInts(options.Item("$").split(":"));
var removeCRLF = false;
if ( args.Length > 1     ) {
   replace = args.Item(1);
   removeCRLF = (block == "\\r\\n") && (replace == "");
   if ( replace.substr(0,1) == "=" ) replace = env(replace.substr(1));
   replace = eval('"' + replace + '"');
   if ( quote != undefined ) replace = replace.replace(eval("/"+quote+"/g"),'"');
   if ( options.Exists("A") ) {  // Enable alternation replacements from "Se|ar|ch" to "Re|pla|ce"
      var Asearch = search.split("|"),
          Areplace = replace.split("|"),
          repl = new Array();
      for ( var i = 0; i < Asearch.length; i++ ) {
         repl[Asearch[i]] = Areplace[i];
      }
      replace = function($0,$1,$2) { return repl[$0]; };
      Asearch.length = 0;
      Areplace.length = 0;
   }
}
if ( search != undefined ) search = new RegExp(search, "gm"+ignoreCase);
if ( block  != undefined ) block  = new RegExp(block , "gm"+ignoreCase);



// FINDREPL [/I] [/V] [/N] rSearch [/E:rEndBlk] [/O:s:e] [/B:rBlock] [/$:s1...]
//          [[/R] [/A] sReplace] [/Q:c] [/S:sSource]

//          In Search and Replace operations:
//            /V or /N switches implies line processing
//            /E or /O switches implies block (and line) processing
//          If Search operation (with no previous switches) have NOT /$ switch:
//            implies line processing (otherwise is file processing)


if ( options.Exists("S") ) {  // Process Source string instead of file
   var source = options.Item("S");
   if ( source.substr(0,1) == "=" ) source = env(source.substr(1));
   var fileContents = new Array(), lastLine = 1;
   fileContents[0] = source;
   procLines = true;
} else {  // Process Stdin file
// -> positive justification omitted

fileContents = WScript.StdIn.ReadAll();

if ( notMatched || showNumber || procBlocks ) procLines = true;
if ( replace==undefined && submatches==undefined ) procLines = true;

if ( procLines ) {  // Separate file contents in lines
   var lastByte = fileContents.slice(-1);
   fileContents = fileContents.replace(/([^\r\n]*)\r?\n/g,"$1\n").match(/^.*$/gm);
   lastLine = fileContents.length - ((lastByte == "\n")?1:0);
}

if ( procBlocks ) {  // Create blocks of lines
   if ( search != undefined ) {  // Blocks based on Search lines:
      if ( offset == undefined ) offset = new Array(0,0);
      for ( var i = 1; i <= lastLine; i++ ) {
         if ( match(fileContents[i-1],search) ) {
            if ( endBlk != undefined ) {  // 1- from Search line to EndBlk line [+offsets].
               for ( var j=i+1; j<=lastLine && !match(fileContents[j-1],endBlk); j++ );
               if ( j <= lastLine ) {
                  var s = i+offset[0], e = j+offset[1];
                  // Insert additional code here to cancel overlapped blocks
                  range.push(s>0?s:1, e>0?e:1);
               }
               i = j;
            } else {  // 2- surrounding Search lines with offsets.
               s = i+offset[0], e = i+offset[1];
               range.push(s>0?s:1, e>0?e:1);
            }
         }
      }
   } else {  // Offset with no Search: block is range of lines
      if ( offset.length < 2 ) offset[1] = lastLine;
      s = offset[0]<0 ? offset[0]+lastLine+1 : offset[0];
      e = offset[1]<0 ? offset[1]+lastLine+1 : offset[1];
      range.push(s>0?s:1, e>0?e:1);
   }
   if ( range.length == 0 ) WScript.Quit(0);
   range.push(0xFFFFFFFF,0xFFFFFFFF);
}

// <- negative justification omitted
}
// endif Process Source string instead of file

if ( replace == undefined ) {  // Search operations
   if ( procLines ) {  // Search on lines
   // -> positive justification omitted...
   if ( procBlocks ) {  // Process previously created blocks
      for ( var r=0, lineNumber=1; lineNumber <= lastLine; lineNumber++ ) {
         if ( (range[r]<=lineNumber && lineNumber<=range[r+1]) != notMatched ) {
            if ( submatches != undefined ) {
               if ( showNumber ) WScript.Stdout.Write(lineNumber+":");
               while ( (nextMatch = block.exec(fileContents[lineNumber-1])) != null ) {
                  for ( var s = 0; s < submatches.length; s++ ) {
                     WScript.Stdout.Write(" " + (quote!=undefined?quote:'"') + 
                                                nextMatch[submatches[s]] + 
                                                (quote!=undefined?quote:'"'));
                  }
                  result++;
               }
               WScript.Stdout.WriteLine();
            } else {
               if ( block == undefined  ||  match(fileContents[lineNumber-1],block) ) {
                  if ( showNumber ) WScript.Stdout.Write(lineNumber+":");
                  WScript.Stdout.WriteLine(fileContents[lineNumber-1]);
                  result++;
               }
            }
         }
         if ( lineNumber >= range[r+1] ) r += 2;
      }
   } else {  // Process all lines for Search
      for ( lineNumber = 1; lineNumber <= lastLine; lineNumber++ ) {
         if ( match(fileContents[lineNumber-1],search) != notMatched ) {
            if ( showNumber ) WScript.Stdout.Write(lineNumber+":");
            if ( submatches != undefined ) {
               search.lastIndex = 0;
               while ( (nextMatch = search.exec(fileContents[lineNumber-1])) != null ) {
                  for ( var s = 0; s < submatches.length; s++ ) {
                     WScript.Stdout.Write(" " + (quote!=undefined?quote:'"') +
                                                nextMatch[submatches[s]] +
                                                (quote!=undefined?quote:'"'));
                  }
                  result++;
               }
               WScript.Stdout.WriteLine();
            } else {
               WScript.Stdout.WriteLine(fileContents[lineNumber-1]);
               result++;
            }
         }
      }
   }
   // <- negative justification omitted...

   } else {  // Search on entire file and show submatched substrings
      if ( submatches != undefined ) {
         while ( (nextMatch = search.exec(fileContents)) != null ) {
            for ( var s = 0; s < submatches.length; s++ ) {
               WScript.Stdout.Write(" " + (quote!=undefined?quote:'"') +
                                          nextMatch[submatches[s]] +
                                          (quote!=undefined?quote:'"'));
            }
            result++;
            WScript.Stdout.WriteLine();
         }
      }
   }
} else {  // Replace operations
   if ( procLines ) {  // Replace on lines
   // -> positive justification omitted...
   if ( procBlocks ) {  // Process previously created blocks
      if ( block == undefined ) block = search;  // Replace rSearch or rBlock (the last one)
      var CRLFremoved = false;
      for ( var r=0, lineNumber=1; lineNumber <= lastLine; lineNumber++ ) {
         if ( range[r]<=lineNumber && lineNumber<=range[r+1] ) {
         if ( removeCRLF ) {
            WScript.Stdout.Write(fileContents[lineNumber-1]);
            CRLFremoved = true;
            result++;
         } else {
            if ( match(fileContents[lineNumber-1],block) ) {
               if ( CRLFremoved ) { WScript.Stdout.WriteLine(); CRLFremoved = false; }
               WScript.Stdout.WriteLine(fileContents[lineNumber-1].replace(block,replace));
               result++;
            } else {
               if ( CRLFremoved ) { WScript.Stdout.WriteLine(); CRLFremoved = false; }
               if ( ! justReplaced ) WScript.Stdout.WriteLine(fileContents[lineNumber-1]);
            }
         }
         } else {
            if ( CRLFremoved ) { WScript.Stdout.WriteLine(); CRLFremoved = false; }
            if ( ! justReplaced ) WScript.Stdout.WriteLine(fileContents[lineNumber-1]);
         }
         if ( lineNumber >= range[r+1] ) r += 2;
      }
      if ( CRLFremoved ) { WScript.Stdout.WriteLine(); CRLFremoved = false; }
   } else {  // Process all lines for Replace
      for ( lineNumber = 1; lineNumber <= lastLine; lineNumber++ ) {
         if ( match(fileContents[lineNumber-1],search) ) {
            WScript.Stdout.WriteLine(fileContents[lineNumber-1].replace(search,replace));
            result++;
         } else {
            if ( ! justReplaced ) WScript.Stdout.WriteLine(fileContents[lineNumber-1]);
         }
      }
   }
   // <- negative justification omitted...

   } else {  // Replace on entire file
      WScript.Stdout.Write(fileContents.replace(search,replace));
   }
}
WScript.Quit(result);