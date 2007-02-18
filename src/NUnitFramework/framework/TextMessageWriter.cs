// *****************************************************
// Copyright 2007, Charlie Poole
// Licensed under the NUnit License, see license.txt
// *****************************************************

using System;
using System.IO;
using System.Text;
using System.Collections;

namespace NUnit.Framework
{
	/// <summary>
	/// TextMessageWriter writes constraint descriptions and messages
	/// in displayable form as a text stream. It tailors the display
	/// of individual message components to form the standard message
	/// format of NUnit assertion failure messages.
	/// </summary>
    public class TextMessageWriter : MessageWriter
    {
        #region Message Formats and Constants
        private static readonly int MAX_LINE_LENGTH = 78;

        private static readonly string Fmt_Connector = " {0} ";
        private static readonly string Fmt_Predicate = "{0} ";
        //private static readonly string Fmt_Label = "{0}";

        private static readonly string Fmt_Null = "null";
        private static readonly string Fmt_EmptyString = "<string.Empty>";
        private static readonly string Fmt_EmptyCollection = "<empty>";

        private static readonly string Fmt_String = "\"{0}\"";
        private static readonly string Fmt_Char = "'{0}'";
        private static readonly string Fmt_ValueType = "{0}";
        private static readonly string Fmt_Default = "<{0}>";
        #endregion

        #region Constructors
		/// <summary>
		/// Construct a TextMessageWriter
		/// </summary>
        public TextMessageWriter() { }

        /// <summary>
        /// Construct a TextMessageWriter, specifying a user message
        /// and optional formatting arguments.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <param name="args"></param>
		public TextMessageWriter(string userMessage, params object[] args)
        {
			if ( userMessage != null && userMessage != string.Empty)
				this.WriteMessageLine(userMessage, args);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the maximum line length for this writer
        /// </summary>
        public override int MaxLineLength
        {
            get { return MAX_LINE_LENGTH; }
        }
        #endregion

        #region Public Methods - High Level
        /// <summary>
        /// Method to write single line  message with optional args, usually
        /// written to precede the general failure message, at a givel 
        /// indentation level.
        /// </summary>
        /// <param name="level">The indentation level of the message</param>
        /// <param name="message">The message to be written</param>
        /// <param name="args">Any arguments used in formatting the message</param>
        public override void WriteMessageLine(int level, string message, params object[] args)
        {
            if (message != null)
            {
                while (level-- >= 0) Write("  ");

                if (args != null && args.Length > 0)
                    message = string.Format(message, args);

                WriteLine(message);
            }
        }

        /// <summary>
        /// Display Expected and Actual lines for a constraint. This
        /// is called by MessageWriter's default implementation of 
        /// WriteMessageTo and provides the generic two-line display. 
        /// </summary>
        /// <param name="constraint">The constraint that failed</param>
        /// <param name="actual">The actual value causing the failure</param>
        public override void DisplayDifferences(IConstraint constraint)
        {
            WriteExpectedLine(constraint);
            WriteActualLine(constraint);
        }

		/// <summary>
		/// Display Expected and Actual lines for given values. This
		/// method may be called by constraints that need more control over
		/// the display of actual and expected values than is provided
		/// by the default implementation.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value causing the failure</param>
		public override void DisplayDifferences(object expected, object actual)
		{
			WriteExpectedLine(expected);
			WriteActualLine(actual);
		}

		/// <summary>
		/// Display Expected and Actual lines for given values, including
		/// a tolerance value on the expected line.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="actual">The actual value causing the failure</param>
		/// <param name="tolerance">The tolerance within which the test was made</param>
		public override void DisplayDifferences(object expected, object actual, object tolerance)
		{
			WriteExpectedLine(expected, tolerance);
			WriteActualLine(actual);
		}

		/// <summary>
        /// Display the expected and actual string values on separate lines.
        /// If the mismatch parameter is >=0, an additional line is displayed
        /// line containing a caret that points to the mismatch point.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual string value</param>
        /// <param name="mismatch">The point at which the strings don't match or -1</param>
        public override void DisplayStringDifferences(string expected, string actual, int mismatch)
        {
            // Maximum string we can display without truncating
            int maxStringLength = MAX_LINE_LENGTH
                - Msgs.PrefixLength   // Allow for prefix
                - 2;                    // 2 quotation marks

            mismatch = MsgUtils.ClipExpectedAndActual(
                ref expected, ref actual, maxStringLength, mismatch, true );

            DisplayDifferences(expected, actual);
            if (mismatch >= 0)
                WriteCaretLine(mismatch);
        }
        #endregion

        #region Public Methods - Low Level
		/// <summary>
		/// Writes the text for a connector.
		/// </summary>
		/// <param name="connector">The connector.</param>
		public override void WriteConnector(string connector)
        {
            Write(Fmt_Connector, connector);
        }

		/// <summary>
		/// Writes the text for a predicate.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		public override void WritePredicate(string predicate)
        {
            Write(Fmt_Predicate, predicate);
        }

        //public override void WriteLabel(string label)
        //{
        //    Write(Fmt_Label, label);
        //}

		/// <summary>
		/// Writes the text for an expected value.
		/// </summary>
		/// <param name="expected">The expected value.</param>
		public override void WriteExpectedValue(object expected)
        {
            WriteValue(expected);
        }

		/// <summary>
		/// Writes the text for an actual value.
		/// </summary>
		/// <param name="actual">The actual value.</param>
		public override void WriteActualValue(object actual)
        {
            WriteValue(actual);
        }

		/// <summary>
		/// Writes the text for a generalized value.
		/// </summary>
		/// <param name="val">The value.</param>
		public override void WriteValue(object val)
        {
            if (val == null)
                Write(Fmt_Null);
            else if (val.GetType().IsArray)
                WriteArray((Array)val);
            else if (val is ICollection)
                WriteCollection((ICollection)val);
            else if (val is string)
                WriteString((string)val);
            else if (val is char)
                WriteChar((char)val);
            else if (val is double)
                WriteDouble((double)val);
            else if (val is float)
                WriteFloat((float)val);
            else if (val is decimal)
                WriteDecimal((decimal)val);
            else if (val.GetType().IsValueType)
                Write(Fmt_ValueType, val);
            else
                Write(Fmt_Default, val);
        }

        private void WriteArray(Array array)
        {
			if ( array.Length == 0 )
			{
				Write( Fmt_EmptyCollection );
				return;
			}
			
			int rank = array.Rank;
            int[] products = new int[rank];

            for (int product = 1, r = rank; --r >= 0; )
                products[r] = product *= array.GetLength(r);

            int count = 0;
            foreach (object obj in array)
            {
                if (count > 0)
                    Write(", ");

                bool startSegment = false;
                for (int r = 0; r < rank; r++)
                {
                    startSegment = startSegment || count % products[r] == 0;
                    if (startSegment) Write("< ");
                }

                WriteValue(obj);

                ++count;

                bool nextSegment = false;
                for (int r = 0; r < rank; r++)
                {
                    nextSegment = nextSegment || count % products[r] == 0;
                    if (nextSegment) Write(" >");
                }
            }
        }

        private void WriteCollection(ICollection collection)
        {
			if ( collection.Count == 0 )
			{
				Write(Fmt_EmptyCollection);
				return;
			}

            int count = 0;
            Write("< ");
            foreach (object obj in collection)
            {
                if (count > 0)
                    Write(", ");
                WriteValue(obj);
                ++count;
            }
            Write(" >");
        }

        private void WriteString(string s)
        {
            if (s == string.Empty)
                Write(Fmt_EmptyString);
            else
                Write(Fmt_String, s);
        }

        private void WriteChar(char c)
        {
            Write(Fmt_Char, c);
        }

        private void WriteDouble(double d)
        {

            if (double.IsNaN(d) || double.IsInfinity(d))
                Write(d);
            else
            {
                string s = d.ToString("G17");

                if (s.IndexOf('.') > 0)
                    Write(s + "d");
                else
                    Write(s + ".0d");
            }
        }

        private void WriteFloat(float f)
        {
            if (float.IsNaN(f) || float.IsInfinity(f))
                Write(f);
            else
            {
                string s = f.ToString("G9");

                if (s.IndexOf('.') > 0)
                    Write(s + "f");
                else
                    Write(s + ".0f");
            }
        }

        private void WriteDecimal(Decimal d)
        {
            Write(d.ToString("G29") + "m");
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Write the generic 'Expected' line for a constraint
        /// </summary>
        /// <param name="constraint">The constraint that failed</param>
        private void WriteExpectedLine(IConstraint constraint)
        {
            Write(Msgs.Pfx_Expected);
            constraint.WriteDescriptionTo(this);
            WriteLine();
        }

		/// <summary>
		/// Write the generic 'Expected' line for a given value
		/// </summary>
		/// <param name="expected">The expected value</param>
		private void WriteExpectedLine(object expected)
		{
			Write(Msgs.Pfx_Expected);
			WriteExpectedValue(expected);
			WriteLine();
		}

		/// <summary>
		/// Write the generic 'Expected' line for a given value
		/// and tolerance.
		/// </summary>
		/// <param name="expected">The expected value</param>
		/// <param name="tolerance">The tolerance within which the test was made</param>
		private void WriteExpectedLine(object expected, object tolerance)
		{
			Write(Msgs.Pfx_Expected);
			WriteExpectedValue(expected);
			WriteConnector( "+/-" );
			WriteExpectedValue(tolerance);
			WriteLine();
		}

		/// <summary>
		/// Write the generic 'Actual' line for a constraint
		/// </summary>
		/// <param name="actual">The actual value causing a failure</param>
		private void WriteActualLine(IConstraint constraint)
		{
			Write(Msgs.Pfx_Actual);
			constraint.WriteActualValueTo(this);
			WriteLine();
		}

		/// <summary>
		/// Write the generic 'Actual' line for a given value
		/// </summary>
		/// <param name="actual">The actual value causing a failure</param>
		private void WriteActualLine(object actual)
		{
			Write(Msgs.Pfx_Actual);
			WriteActualValue(actual);
			WriteLine();
		}

		private void WriteCaretLine(int mismatch)
        {
            // We subtract 2 for the initial 2 blanks and add back 1 for the initial quote
            WriteLine("  {0}^", new string('-', Msgs.PrefixLength + mismatch - 2 + 1));
        }
        #endregion
    }
}