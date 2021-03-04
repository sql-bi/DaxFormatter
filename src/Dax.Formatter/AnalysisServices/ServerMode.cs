namespace Dax.Formatter.AnalysisServices
{
	/// <summary>
	/// Specifies the server mode used. For more information about server modes and how to set the server deployment mode, see Enable a Standalone VertiPAq Engine Instance.
	/// </summary>
	public enum ServerMode
	{
		/// <summary>
		/// Classic OLAP and Data Mining mode.
		/// </summary>
		/// <!-- AS 2005 and AS 2008 -->
		Multidimensional,
		/// <summary>
		/// SharePoint Integration mode.
		/// </summary>
		/// <!-- AS 2008 R2 -->
		SharePoint,
		/// <summary>
		/// Specifies that the storage mode is proprietary Analysis Services xVelocity in-memory analytics engine (VertiPaq).
		/// </summary>
		/// <!-- IMBI -->
		Tabular,
		/// <summary>
		/// ???
		/// </summary>
		Default
	}
}
