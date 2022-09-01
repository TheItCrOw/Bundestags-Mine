namespace BundestagMine.ViewModels.GlobalSearch
{
    public class GlobalSearchResultViewModel
    {
        /// <summary>
        /// The object holding the results like speeches, speakers, polls, agendaitems. Depends on the view
        /// </summary>
        public object ResultList { get; set; }

        /// <summary>
        /// The current page we are showing
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// The total amounts of results found
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// The amount of results in each page.
        /// </summary>
        public int TakeResults { get; set; }

        public string SearchString { get; set; }

        public ResultType Type { get; set; }
    }

    public enum ResultType
    {
        Speeches,
        Shouts,
        Speakers,
        AgendaItems,
        Polls
    }
}
