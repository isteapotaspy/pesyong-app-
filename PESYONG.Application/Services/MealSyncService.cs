using System;

namespace PESYONG.ApplicationLogic.Services
{
    /// <summary>
    /// Provides a simple notification mechanism for meal data changes
    /// so subscribed UI components can refresh their displayed meal lists.
    /// </summary>
    public class MealSyncService
    {
        /// <summary>
        /// Occurs when meal data has changed.
        /// </summary>
        public event Action? MealsChanged;

        /// <summary>
        /// Raises the <see cref="MealsChanged"/> event to notify subscribers
        /// that meal data should be refreshed.
        /// </summary>
        public void NotifyMealsChanged()
        {
            MealsChanged?.Invoke();
        }
    }
}