using System;
using System.Collections;
using System.Reflection;
using NUnit.Core;
using NUnit.Core.Extensibility;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for AddinRegistry.
	/// </summary>
	public class AddinRegistry : MarshalByRefObject, IAddinRegistry, IService
    {
        #region Instance Fields
        private ArrayList addins = new ArrayList();
		#endregion

		#region IAddinRegistry Members

		public void Register(Addin addin)
		{
			addins.Add( addin );
		}

		public  IList Addins
		{
			get
			{
				return addins;
			}
		}

		public void SetStatus( string name, AddinStatus status )
		{
			foreach( Addin addin in addins )
				if ( addin.Name == name )
					addin.Status = status;
		}
		#endregion

		#region IService Members
		public void InitializeService()
		{
		}

		public void UnloadService()
		{
		}
		#endregion
	}
}