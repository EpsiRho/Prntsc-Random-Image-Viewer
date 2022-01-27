using RabbitHole.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHole.ViewModels
{
    public class RecentViewModel
    {
        private ObservableCollection<Recent> recents = new ObservableCollection<Recent>();
        public ObservableCollection<Recent> Recents { get { return this.recents; } }
        public RecentViewModel() { }

        public List<Recent> ThumbnailAquired { get; set; }

        public void AddRecent(Recent a)
        {
            recents.Insert(0, a);
        }
    }
}
