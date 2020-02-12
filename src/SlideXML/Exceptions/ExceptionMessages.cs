﻿namespace SlideXML.Exceptions
{
    /// <summary>
    /// Contains constant error messages.
    /// </summary>
    public static class ExceptionMessages
    {
        public static string NoTextFrame = "Element has not a text frame.";

        public static string NoChart = "Element has not a chart.";

        public static string NotPlaceholder = "Element is not a placeholder";

        public static string NotTitle = "Chart has not a title.";

        /// <summary>
        /// Returns message string with placeholder.
        /// </summary>
        public static string PresentationIsLarge = "The size of presentation more than {0} bytes.";

        public static string SlidesMuchMore = "The number of slides is more allowed {0}.";
    }
}
