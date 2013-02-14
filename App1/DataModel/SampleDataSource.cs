using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace App1.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : App1.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ApHpRunes = "Masteries are 21/0/9.\nGreat Mark of Magic Penetration for reds.\nGreater Seal of Scaling Health for yellows.\nGreater Glyph of Scaling Ability Power for blues.\nGreater Quintessence of Ability Power for quints.\nIf you wish yellows can be switched out for mana regeneration and blues for magic resist if you feel like the lane will be tough.";
            String ADCRunes =
                "Masteries are 21/9/0.\nRune set up is Greater Mark of Attack Damage for reds.\nGreater Seal of Armor for yellow\nGreater Glypg of Scaling Magic Resist for blues.\nGreater Quintessence of Attack Damage for quints.";
            String Olaf =
                "Masteries are 9/21/0.\n7 Greater Mark of Armor Penetration and 2 Greater Mark of Attack Damage for reds.\nGreater Seal of Armor for yellows.\nGreater Glyph of Magic Resist for blues. Greater Quintessence of Attack Damage for quints.";
            String ApMana =
                "Masteries are 21/0/9.\nGreater Mark of Insight for reds.\nGreater Seal of Replenishment for yellows.\nGreater Glyph of Warding for blues.\nGreater Quintessence of Ability Power for quints";
            String FlashIgnite = "Flash and Ignite are the optimal choices";
            String FlashSmite = "Flash and Smite are the optimal choices";

            String ApHpMSRunes = "Masteries are 21/0/9.\nGreat Mark of Magic Penetration for reds.\nGreater Seal of Scaling Health for yellows.\nGreater Glyph of Scaling Ability Power for blues.\nGreater Quintessence of Swiftness for quints.";
            String ArmorMS = "Masteries are 0/21/9.\nGreat Mark of Magic Penetration for reds.\nGreater Seal of Armor for yellows.\nGreater Glyph of Magic Resist for blues.\nGreater Quintessence of Swiftness for quints.";

            var group1 = new SampleDataGroup("Group-1",
                    "Gragas",
                    "Mid Lane",
                    "Assets/gragas.png",
                    "Group Description: ");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/Ignite.png",
                    FlashIgnite,
                    ApHpRunes,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Items",
                    "",
                    "Assets/rabadons-deathcap.png",
                    "Recommended Build Order",
                    "1. Boots+ 3 pots \n" +
                    "2. Dorans Ring x2\n" +
                    "3. Haunting Guise\n" +
                    "4. Death Cap\n" +
                    "5. Zhonya's Hourglass\n" +
                    "6. Void Staff\n" +
                    "7. Upgrade Haunting Guise to Liandry\n" +
                    "8. Sell Dorans for either more damage or GA\n",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/akali.png",
                    "Counters",
                    "Brand, Akali, and Fizz counter Gragas well.",
                    group1));

            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                     "Tristana",
                     "AD Carry Bot Lane",
                     "Assets/tristana.jpg",
                     "Group Description: ");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/Ignite.png",
                    "Flash and ignite are the optimal choices. Ignite can be swapped out for Heal or Cleanse.",
                    ADCRunes,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Items",
                    "",
                    "Assets/Infinity_Edge.jpg",
                    "Recommended Build Order",
                    "1. Boots+ 3 pots \n" +
                    "2.(First Back): Dorans Ring + Vampiric Scepter\n" +
                    "3. Infinity Edge\n" +
                    "4. Phantom Dancer\n" +
                    "5. Last Whisper\n" +
                    "6. Quicksilver Sash\n" +
                    "7. Bloodthrister\n" +
                    "8. Upgrade QSS\n" +
                    "9. Sell Dorans for GA",

                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/mf.jpg",
                    "Counters",
                    "Miss Fortune, Caitlyn, and Sivir counter Tristana early.",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                "Olaf",
                "Top Lane Bruiser",
                "Assets/olaf.jpg",
                "Group Description: ");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/teleport.jpg",
                    "Ghost and Teleport should be used for top lane. You can switch out Ignite for Teleport if you want more early kill pressure.",
                    Olaf,
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Items",
                    "",
                    "Assets/warmogs-armor.png",
                    "Recommended Build Order",
                    "1. Boots+ 3 pots \n" +
                    "2.(First Back): Dorans Bladex2 OR Phage OR Brutalizer\n" +
                    "3. Shurelias\n" +
                    "4. Black Cleaver\n" +
                    "5. Warmogs\n" +
                    "6. Frozen Mallet\n" +
                    "7. Randuins\n" +
                    "8. Maw of Malmortius\n" +
                    "9. Sell Dorans for GA",

                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/darius.jpg",
                    "Counters",
                    "Olaf is hard to beat in lane. Darius and Riven put up tougher matchups.",
                    group3));
            this.AllGroups.Add(group3);

            var group4 = new SampleDataGroup("Group-4",
                      "Lee Sin",
                      "Jungle",
                      "Assets/leesin.jpg",
                      "Group Description: ");
            group4.Items.Add(new SampleDataItem("Group-4-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/smite.jpg",
                    "Smite and Flash are standard. Can swap out flash for exhaust if desired.",
                    Olaf,
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-2",
                    "Items",
                    "",
                    "Assets/wriggles.png",
                    "Recommended Build Order",
                    "1. Hunter's Machete+ 5 pots \n" +
                    "2. Wriggle's Lantern\n" +
                    "3. Randuins\n" +
                    "4. Black Cleaver\n" +
                    "5. Frozen Mallet\n" +
                    "6. Maw of Malmortius\n" +
                    "7. Black Cleaver\n" +
                    "8. GA\n",

                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/riven.png",
                    "Counters",
                    "Lee Sin is a very strong duelist. Played correctly he can win almost all 1v1s. Riven does give him a hard time though.",
                    group4));
            this.AllGroups.Add(group4);

            var group5 = new SampleDataGroup("Group-5",
                        "Orianna",
                        "Mid Lane",
                        "Assets/orianna.jpg",
                        "Group Description: ");
            group5.Items.Add(new SampleDataItem("Group-5-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/Ignite.png",
                    FlashIgnite,
                    ApMana,
                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-2",
                    "Items",
                    "",
                    "Assets/athenes.png",
                    "Recommended Build Order",
                    "1. Boots + 3 pots \n" +
                    "2. Athene's Unholy Grail\n" +
                    "3. Rabadon's Deathcap\n" +
                    "4. Haunting Guise\n" +
                    "5. Void Staff\n" +
                    "6. Zhonya's Hourglass\n" +
                    "7. Upgrade Haunting Guise to Liandry's Torment\n",

                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/kassadin.png",
                    "Counters",
                    "Kassadin, Cassiopeia, Leblanc, and Ahri are all good counters to Orianna",
                    group5));
            this.AllGroups.Add(group5);

            var group6 = new SampleDataGroup("Group-6",
              "Nunu",
              "Support",
              "Assets/nunuf.jpg",
              "Group Description: ");
            group6.Items.Add(new SampleDataItem("Group-6-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/exhaust.png",
                    "Flash and Exhaust are standard. Can also go with Flash and Heal.",
                    "For Masteries go 1/12/17.\nGreater Mark of Resilience on reds.\nGreater Seal of Avarice on yellows.\nGreater Glyph of Warding on blues.\nGreater Quintessence of Avarice on quints.",
                    group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-2",
                    "Items",
                    "",
                    "Assets/aegis.png",
                    "Recommended Build Order",
                    "1. Boots + wards \n" +
                    "2. Sightstone\n" +
                    "3. Shurelias\n" +
                    "4. Aegis\n" +
                    "5. Zeke's Herald\n" +
                    "6. Locket of Iron Solari\n" +
                    "7. Kage's Last Breath\n",

                    group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/soraka.jpg",
                    "Counters",
                    "Soraka, Sona, Janna and Sivir",
                    group6));
            this.AllGroups.Add(group6);

            var group7 = new SampleDataGroup("Group-7",
                      "Ziggs",
                      "Mid Lane",
                      "Assets/Ziggs.jpg",
                      "Group Description: ");
            group7.Items.Add(new SampleDataItem("Group-7-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/Ignite.png",
                    FlashIgnite,
                    ApMana,
                    group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-2",
                    "Items",
                    "",
                    "Assets/athenes.png",
                    "Recommended Build Order",
                    "1. Boots + 3 pots \n" +
                    "2. Dorans Ringx2\n" +
                    "3. Athene's Unholy Grail\n" +
                    "4. Rabadon's Deathcap\n" +
                    "5. Rylai's Crystal Scepter\n" +
                    "6. Void Staff\n" +
                    "7. Zhonya's Hourglass\n",

                    group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/ahri.png",
                    "Counters",
                    "Kassadin, Leblanc, and Ahri",
                    group7));
            this.AllGroups.Add(group7);

            var group8 = new SampleDataGroup("Group-8",
                     "Twisted Fate",
                     "Mid Lane",
                     "Assets/tf.jpg",
                     "Group Description: ");
            group8.Items.Add(new SampleDataItem("Group-8-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/Ignite.png",
                    FlashIgnite,
                    ApHpMSRunes,
                    group8));
            group8.Items.Add(new SampleDataItem("Group-8-Item-2",
                    "Items",
                    "",
                    "Assets/lich.png",
                    "Recommended Build Order",
                    "1. Cystalline Flak + HP potx2 + Mana pot \n" +
                    "2. Dorans Ringx2\n" +
                    "3. Lich Bane\n" +
                    "4. Rabadon's Deathcap\n" +
                    "5. Zhonya's Hourglass\n" +
                    "6. Void Staff\n" +
                    "7. Guardian Angel\n",

                    group8));
            group8.Items.Add(new SampleDataItem("Group-8-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/lb.png",
                    "Counters",
                    "Kassadin, Leblanc, and Talon are all good counters to Twisted Fate.\nAlso, when TF leaves lane with ult, push to his tower to make him xp and cs.",
                    group8));
            this.AllGroups.Add(group8);

            var group9 = new SampleDataGroup("Group-9",
                    "Amumu",
                    "Jungle",
                    "Assets/mummy.jpg",
                    "Group Description: ");
            group9.Items.Add(new SampleDataItem("Group-9-Item-1",
                    "Runes, Masteries and Summoner Spells",
                    "",
                    "Assets/smite.jpg",
                    FlashSmite,
                    ArmorMS,
                    group9));
            group9.Items.Add(new SampleDataItem("Group-9-Item-2",
                    "Items",
                    "",
                    "Assets/sunfire.png",
                    "Recommended Build Order",
                    "1. Hunter's Machete + HP potsx5\n" +
                    "2. Spirit Stone + Boots\n" +
                    "3. Sunfire Cape\n" +
                    "4. Abyssal Scepter\n" +
                    "5. Frozen Heart\n" +
                    "6. Randuin's Omen\n" +
                    "7. Zhonya's Hourglass\n",

                    group9));
            group9.Items.Add(new SampleDataItem("Group-9-Item-3",
                   "Counter Picks",
                    "",
                    "Assets/mundo.png",
                    "Counters",
                    "Dr. Mundo, Lee Sin, Nocturne.\nInvade his blue level 1.",
                    group9));
            this.AllGroups.Add(group9);

        }
    }
}
