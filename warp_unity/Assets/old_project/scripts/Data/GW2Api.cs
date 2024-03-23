using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mirror;


public class GW2Api : MonoBehaviour
{
    #region Long Ass dictIdToMap
    public static Dictionary<int, string> dictIdToMap = new Dictionary<int, string>()
    {
        { 15, "Queensdale" },
        { 17, "Harathi Hinterlands" },
        { 18, "Divinity's Reach" },
        { 19, "Plains of Ashford" },
        { 20, "Blazeridge Steppes" },
        { 21, "Fields of Ruin" },
        { 22, "Fireheart Rise" },
        { 23, "Kessex Hills" },
        { 24, "Gendarran Fields" },
        { 25, "Iron Marches" },
        { 26, "Dredgehaunt Cliffs" },
        { 27, "Lornar's Pass" },
        { 28, "Wayfarer Foothills" },
        { 29, "Timberline Falls" },
        { 30, "Frostgorge Sound" },
        { 31, "Snowden Drifts" },
        { 32, "Diessa Plateau" },
        { 33, "Ascalonian Catacombs" },
        { 34, "Caledon Forest" },
        { 35, "Metrica Province" },
        { 36, "Ascalonian Catacombs" },
        { 37, "Arson at the Orphanage" },
        { 38, "Eternal Battlegrounds" },
        { 39, "Mount Maelstrom" },
        { 50, "Lion's Arch" },
        { 51, "Straits of Devastation" },
        { 53, "Sparkfly Fen" },
        { 54, "Brisban Wildlands" },
        { 55, "The Hospital in Jeopardy" },
        { 61, "Infiltration" },
        { 62, "Cursed Shore" },
        { 63, "Sorrow's Embrace" },
        { 64, "Sorrow's Embrace" },
        { 65, "Malchor's Leap" },
        { 66, "Citadel of Flame" },
        { 67, "Twilight Arbor" },
        { 68, "Twilight Arbor" },
        { 69, "Citadel of Flame" },
        { 70, "Honor of the Waves" },
        { 71, "Honor of the Waves" },
        { 73, "Bloodtide Coast" },
        { 75, "Caudecus's Manor" },
        { 76, "Caudecus's Manor" },
        { 77, "Search the Premises" },
        { 79, "The Informant" },
        { 80, "A Society Function" },
        { 81, "Crucible of Eternity" },
        { 82, "Crucible of Eternity" },
        { 89, "Chasing the Culprits" },
        { 91, "The Grove" },
        { 92, "The Trial of Julius Zamon" },
        { 95, " Alpine Borderlands" },
        { 96, " Alpine Borderlands" },
        { 97, "Infiltration" },
        { 110, "The Perils of Friendship" },
        { 111, "Victory or Death" },
        { 112, "The Ruined City of Arah" },
        { 113, "Desperate Medicine" },
        { 120, "The Commander" },
        { 138, "Defense of Shaemoor" },
        { 139, "Rata Sum" },
        { 140, "The Apothecary" },
        { 142, "Going Undercover" },
        { 143, "Going Undercover" },
        { 144, "The Greater Good" },
        { 145, "The Rescue" },
        { 147, "Breaking the Blade" },
        { 148, "The Fall of Falcon Company" },
        { 149, "The Fall of Falcon Company" },
        { 152, "Confronting Captain Tervelan" },
        { 153, "Seek Logan's Aid" },
        { 154, "Seek Logan's Aid" },
        { 157, "Accusation" },
        { 159, "Accusation" },
        { 161, "Liberation" },
        { 162, "Voices From the Past" },
        { 163, "Voices From the Past" },
        { 171, "Rending the Mantle" },
        { 172, "Rending the Mantle" },
        { 178, "The Floating Grizwhirl" },
        { 179, "The Floating Grizwhirl" },
        { 180, "The Floating Grizwhirl" },
        { 182, "Clown College" },
        { 184, "The Artist's Workshop" },
        { 185, "Into the Woods" },
        { 186, "The Ringmaster" },
        { 190, "The Orders of Tyria" },
        { 191, "The Orders of Tyria" },
        { 192, "Brute Force" },
        { 193, "Mortus Virge" },
        { 195, "Triskell Quay" },
        { 196, "Track the Seraph" },
        { 198, "Speaker of the Dead" },
        { 199, "The Sad Tale of the \"Ravenous\"" },
        { 201, "Kellach's Attack" },
        { 202, "The Queen's Justice" },
        { 203, "The Trap" },
        { 211, "Best Laid Plans" },
        { 212, "Welcome Home" },
        { 215, "The Tribune's Call" },
        { 216, "The Tribune's Call" },
        { 217, "The Tribune's Call" },
        { 218, "Black Citadel" },
        { 222, "A Spy for a Spy" },
        { 224, "Scrapyard Dogs" },
        { 225, "A Spy for a Spy" },
        { 226, "On the Mend" },
        { 232, "Spilled Blood" },
        { 234, "Ghostbore Musket" },
        { 237, "Iron Grip of the Legion" },
        { 238, "The Flame Advances" },
        { 239, "The Flame Advances" },
        { 242, "Test Your Metal" },
        { 244, "Quick and Quiet" },
        { 248, "Salma District (Home)" },
        { 249, "An Unusual Inheritance" },
        { 250, "Windrock Maze" },
        { 251, "Mired Deep" },
        { 252, "Mired Deep" },
        { 254, "Deadly Force" },
        { 255, "Ghostbore Artillery" },
        { 256, "No Negotiations" },
        { 257, "Salvaging Scrap" },
        { 258, "Salvaging Scrap" },
        { 259, "In the Ruins" },
        { 260, "In the Ruins" },
        { 262, "Chain of Command" },
        { 263, "Chain of Command" },
        { 264, "Time for a Promotion" },
        { 267, "The End of the Line" },
        { 269, "Magic Users" },
        { 271, "Rage Suppression" },
        { 272, "Rage Suppression" },
        { 274, "Operation: Bulwark" },
        { 275, "AWOL" },
        { 276, "Human's Lament" },
        { 282, "Misplaced Faith" },
        { 283, "Thicker Than Water" },
        { 284, "Dishonorable Discharge" },
        { 287, "Searching for the Truth" },
        { 288, "Lighting the Beacons" },
        { 290, "Stoking the Flame" },
        { 294, "A Fork in the Road" },
        { 295, "Sins of the Father" },
        { 297, "Graveyard Ornaments" },
        { 326, "Hoelbrak" },
        { 327, "Desperate Medicine" },
        { 330, "Seraph Headquarters" },
        { 334, "Keg Brawl" },
        { 335, "Claw Island" },
        { 336, "Chantry of Secrets" },
        { 350, "Heart of the Mists" },
        { 363, "The Sting" },
        { 364, "Drawing Out the Cult" },
        { 365, "Ashes of the Past" },
        { 371, "Hero's Canton (Home)" },
        { 372, "Blood Tribune Quarters" },
        { 373, "The Command Core" },
        { 374, "Knut Whitebear's Loft" },
        { 375, "Hunter's Hearth (Home)" },
        { 376, "Stonewright's Steading" },
        { 378, "Queen's Throne Room" },
        { 379, "The Great Hunt" },
        { 380, "A Weapon of Legend" },
        { 381, "The Last of the Giant-Kings" },
        { 382, "Disciples of the Dragon" },
        { 385, "A Weapon of Legend" },
        { 386, "Echoes of Ages Past" },
        { 387, "Wild Spirits" },
        { 388, "Out of the Skies" },
        { 389, "Echoes of Ages Past" },
        { 390, "Twilight of the Wolf" },
        { 391, "Rage of the Minotaurs" },
        { 392, "A Pup's Illness" },
        { 393, "Through the Veil" },
        { 394, "A Trap Foiled" },
        { 396, "Raven's Revered" },
        { 397, "One Good Drink Deserves Another" },
        { 399, "Shape of the Spirit" },
        { 400, "Into the Mists" },
        { 401, "Through the Veil" },
        { 405, "Blessed of Bear" },
        { 407, "The Wolf Havroun" },
        { 410, "Minotaur Rampant" },
        { 411, "Minotaur Rampant" },
        { 412, "Unexpected Visitors" },
        { 413, "Rumors of Trouble" },
        { 414, "A New Challenger" },
        { 415, "Unexpected Visitors" },
        { 416, "Roadblock" },
        { 417, "Assault on Moledavia" },
        { 418, "Don't Leave Your Toys Out" },
        { 419, "A New Challenger" },
        { 420, "First Attack" },
        { 421, "The Finishing Blow" },
        { 422, "The Semifinals" },
        { 423, "The Championship Fight" },
        { 424, "The Championship Fight" },
        { 425, "The Machine in Action" },
        { 427, "Among the Kodan" },
        { 428, "Rumors of Trouble" },
        { 429, "Rage of the Minotaurs" },
        { 430, "Darkness at Drakentelt" },
        { 432, "Fighting the Nightmare" },
        { 434, "Preserving the Balance" },
        { 435, "Means to an End" },
        { 436, "Dredge Technology" },
        { 439, "Underground Scholar" },
        { 440, "Dredge Assault" },
        { 441, "The Dredge Hideout" },
        { 444, "Sabotage" },
        { 447, "Codebreaker" },
        { 449, "Armaments" },
        { 453, "Assault the Hill" },
        { 454, "Silent Warfare" },
        { 455, "Sever the Head" },
        { 458, "Fury of the Dead" },
        { 459, "A Fork in the Road" },
        { 460, "Citadel Stockade" },
        { 464, "Tribunes in Effigy" },
        { 465, "Sins of the Father" },
        { 466, "Misplaced Faith" },
        { 470, "Graveyard Ornaments" },
        { 471, "Undead Infestation" },
        { 474, "Whispers in the Dark" },
        { 476, "Dangerous Research" },
        { 477, "Digging Up Answers" },
        { 480, "Defending the Keep" },
        { 481, "Undead Detection" },
        { 483, "Ever Vigilant" },
        { 485, "Research and Destroy" },
        { 487, "Whispers of Vengeance" },
        { 488, "Killer Instinct" },
        { 489, "Meeting my Mentor" },
        { 490, "A Fragile Peace" },
        { 492, "Don't Shoot the Messenger" },
        { 496, "Meeting my Mentor" },
        { 497, "Dredging Up the Past" },
        { 498, "Dredging Up the Past" },
        { 499, "Scrapyard Dogs" },
        { 502, "Quaestor's Siege" },
        { 503, "Minister's Defense" },
        { 504, "Called to Service" },
        { 505, "Called to Service" },
        { 507, "Mockery of Death" },
        { 509, "Discovering Darkness" },
        { 511, "Hounds and the Hunted" },
        { 512, "Hounds and the Hunted" },
        { 513, "Loved and Lost" },
        { 514, "Saving the Stag" },
        { 515, "Hidden in Darkness" },
        { 516, "Good Work Spoiled" },
        { 517, "Black Night, White Stag" },
        { 518, "The Omphalos Chamber" },
        { 519, "Weakness of the Heart" },
        { 520, "Awakening" },
        { 521, "Holding Back the Darkness" },
        { 522, "A Sly Trick" },
        { 523, "Deep Tangled Roots" },
        { 524, "The Heart of Nightmare" },
        { 525, "Beneath a Cold Moon" },
        { 527, "The Knight's Duel" },
        { 528, "Hammer and Steel" },
        { 529, "Where Life Goes" },
        { 532, "After the Storm" },
        { 533, "After the Storm" },
        { 534, "Beneath the Waves" },
        { 535, "Mirror, Mirror" },
        { 536, "A Vision of Darkness" },
        { 537, "Shattered Light" },
        { 538, "An Unknown Soul" },
        { 539, "An Unknown Soul" },
        { 540, "Where Life Goes" },
        { 542, "Source of the Issue" },
        { 543, "Wild Growth" },
        { 544, "Wild Growth" },
        { 545, "Seeking the Zalisco" },
        { 546, "The Direct Approach" },
        { 547, "Trading Trickery" },
        { 548, "Eye of the Sun" },
        { 549, "Battle of Kyhlo" },
        { 552, "Seeking the Zalisco" },
        { 554, "Forest of Niflhel" },
        { 556, "A Different Dream" },
        { 557, "A Splinter in the Flesh" },
        { 558, "Shadow of the Tree" },
        { 559, "Eye of the Sun" },
        { 560, "Sharpened Thorns" },
        { 561, "Bramble Walls" },
        { 563, "Secrets in the Earth" },
        { 564, "The Blossom of Youth" },
        { 566, "The Bad Apple" },
        { 567, "Trouble at the Roots" },
        { 569, "Flower of Death" },
        { 570, "Dead of Winter" },
        { 571, "A Tangle of Weeds" },
        { 573, "Explosive Intellect" },
        { 574, "In Snaff's Footsteps" },
        { 575, "Golem Positioning System" },
        { 576, "Monkey Wrench" },
        { 577, "Defusing the Problem" },
        { 578, "The Things We Do For Love" },
        { 579, "The Snaff Prize" },
        { 581, "A Sparkling Rescue" },
        { 582, "High Maintenance" },
        { 583, "Snaff Would Be Proud" },
        { 584, "Taking Credit Back" },
        { 586, "Political Homicide" },
        { 587, "Here, There, Everywhere" },
        { 588, "Piece Negotiations" },
        { 589, "Readings On the Rise" },
        { 590, "Snaff Would Be Proud" },
        { 591, "Readings On the Rise" },
        { 592, "Unscheduled Delay" },
        { 594, "Stand By Your Krewe" },
        { 595, "Unwelcome Visitors" },
        { 596, "Where Credit Is Due" },
        { 597, "Where Credit Is Due" },
        { 598, "Short Fuse" },
        { 599, "Short Fuse" },
        { 606, "Salt in the Wound" },
        { 607, "Free Rein" },
        { 608, "Serving Up Trouble" },
        { 609, "Serving Up Trouble" },
        { 610, "Flash Flood" },
        { 611, "I Smell a Rat" },
        { 613, "Magnum Opus" },
        { 614, "Magnum Opus" },
        { 617, "Bad Business" },
        { 618, "Beta Test" },
        { 619, "Beta Test" },
        { 620, "Any Sufficiently Advanced Science" },
        { 621, "Any Sufficiently Advanced Science" },
        { 622, "Bad Forecast" },
        { 623, "Industrial Espionage" },
        { 624, "Split Second" },
        { 625, "Carry a Big Stick" },
        { 627, "Meeting my Mentor" },
        { 628, "Stealing Secrets" },
        { 629, "A Bold New Theory" },
        { 630, "Forging Permission" },
        { 631, "Forging Permission" },
        { 633, "Setting the Stage" },
        { 634, "Containment" },
        { 635, "Containment" },
        { 636, "Hazardous Environment" },
        { 638, "Down the Hatch" },
        { 639, "Down the Hatch" },
        { 642, "The Stone Sheath" },
        { 643, "Bad Blood" },
        { 644, "Test Subject" },
        { 645, "Field Test" },
        { 646, "The House of Caithe" },
        { 647, "Dreamer's Terrace (Home)" },
        { 648, "The Omphalos Chamber" },
        { 649, "Snaff Memorial Lab" },
        { 650, "Applied Development Lab (Home)" },
        { 651, "Council Level" },
        { 652, "A Meeting of the Minds" },
        { 653, "Mightier than the Sword" },
        { 654, "They Went Thataway" },
        { 655, "Lines of Communication" },
        { 656, "Untamed Wilds" },
        { 657, "An Apple a Day" },
        { 658, "Base of Operations" },
        { 659, "The Lost Chieftain's Return" },
        { 660, "Thrown Off Guard" },
        { 662, "Pets and Walls Make Stronger Kraals" },
        { 663, "Doubt" },
        { 664, "The False God's Lair" },
        { 666, "Bad Ice" },
        { 667, "Bad Ice" },
        { 668, "Pets and Walls Make Stronger Kraals" },
        { 669, "Attempted Deicide" },
        { 670, "Doubt" },
        { 672, "Rat-Tastrophe" },
        { 673, "Salvation Through Heresy" },
        { 674, "Enraged and Unashamed" },
        { 675, "Pastkeeper" },
        { 676, "Protest Too Much" },
        { 677, "Prying the Eye Open" },
        { 678, "The Hatchery" },
        { 680, "Convincing the Faithful" },
        { 681, "Evacuation" },
        { 682, "Untamed Wilds" },
        { 683, "Champion's Sacrifice" },
        { 684, "Thieving from Thieves" },
        { 685, "Crusader's Return" },
        { 686, "Unholy Grounds" },
        { 687, "Chosen of the Sun" },
        { 691, "Set to Blow" },
        { 692, "Gadd's Last Gizmo" },
        { 693, "Library Science" },
        { 694, "Rakt and Ruin" },
        { 695, "Suspicious Activity" },
        { 696, "Reconnaissance" },
        { 697, "Critical Blowback" },
        { 698, "The Battle of Claw Island" },
        { 699, "Suspicious Activity" },
        { 700, "Priory Library" },
        { 701, "On Red Alert" },
        { 702, "Forearmed Is Forewarned" },
        { 703, "The Oratory" },
        { 704, "Killing Fields" },
        { 705, "The Ghost Rite" },
        { 706, "The Good Fight" },
        { 707, "Defense Contract" },
        { 708, "Shards of Orr" },
        { 709, "The Sound of Psi-Lance" },
        { 710, "Early Parole" },
        { 711, "Magic Sucks" },
        { 712, "A Light in the Darkness" },
        { 713, "The Priory Assailed" },
        { 714, "Under Siege" },
        { 715, "Retribution" },
        { 716, "Retribution" },
        { 719, "The Sound of Psi-Lance" },
        { 726, "Wet Work" },
        { 727, "Shell Shock" },
        { 728, "Volcanic Extraction" },
        { 729, "Munition Acquisition" },
        { 730, "To the Core" },
        { 731, "The Battle of Fort Trinity" },
        { 732, "Tower Down" },
        { 733, "Forging the Pact" },
        { 735, "Willing Captives" },
        { 736, "Marshaling the Truth" },
        { 737, "Breaking the Bone Ship" },
        { 738, "Liberating Apatia" },
        { 739, "Liberating Apatia" },
        { 743, "Fixing the Blame" },
        { 744, "A Sad Duty" },
        { 745, "Striking off the Chains" },
        { 746, "Delivering Justice" },
        { 747, "Intercepting the Orb" },
        { 750, "Close the Eye" },
        { 751, "Through the Looking Glass" },
        { 758, "The Cathedral of Silence" },
        { 760, "Starving the Beast" },
        { 761, "Stealing Light" },
        { 762, "Hunters and Prey" },
        { 763, "Romke's Final Voyage" },
        { 764, "Marching Orders" },
        { 766, "Air Drop" },
        { 767, "Estate of Decay" },
        { 768, "What the Eye Beholds" },
        { 769, "Conscript the Dead Ships" },
        { 772, "Ossuary of Unquiet Dead" },
        { 775, "Temple of the Forgotten God" },
        { 776, "Temple of the Forgotten God" },
        { 777, "Temple of the Forgotten God" },
        { 778, "Through the Looking Glass" },
        { 779, "Starving the Beast" },
        { 780, "Against the Corruption" },
        { 781, "The Source of Orr" },
        { 782, "Armor Guard" },
        { 783, "Blast from the Past" },
        { 784, "The Steel Tide" },
        { 785, "Further Into Orr" },
        { 786, "Ships of the Line" },
        { 787, "Source of Orr" },
        { 788, "Victory or Death" },
        { 789, "A Grisly Shipment" },
        { 790, "Blast from the Past" },
        { 792, "A Pup's Illness" },
        { 793, "Hunters and Prey" },
        { 795, "Legacy of the Foefire" },
        { 796, "The Informant" },
        { 797, "A Traitor's Testimony" },
        { 799, "Follow the Trail" },
        { 806, "Awakening" },
        { 807, "Eye of the North" },
        { 820, "The Omphalos Chamber" },
        { 821, "The Omphalos Chamber" },
        { 825, "Codebreaker" },
        { 827, "Caer Aval" },
        { 828, "The Durmand Priory" },
        { 830, "Vigil Headquarters" },
        { 833, "Ash Tribune Quarters" },
        { 845, "Shattered Light" },
        { 862, "Reaper's Rumble" },
        { 863, "Ascent to Madness" },
        { 864, "Lunatic Inquisition" },
        { 865, "Mad King's Clock Tower" },
        { 866, "Mad King's Labyrinth" },
        { 872, "Fractals of the Mists" },
        { 873, "Southsun Cove" },
        { 875, "Temple of the Silent Storm" },
        { 877, "Snowball Mayhem" },
        { 878, "Tixx's Infinirarium" },
        { 880, "Toypocalypse" },
        { 881, "Bell Choir Ensemble" },
        { 882, "Winter Wonderland" },
        { 894, "Spirit Watch" },
        { 895, "Super Adventure Box" },
        { 896, "North Nolan Hatchery" },
        { 897, "Cragstead" },
        { 899, "Obsidian Sanctum" },
        { 900, "Skyhammer" },
        { 905, "Crab Toss" },
        { 911, "Dragon Ball Arena" },
        { 914, "The Dead End" },
        { 918, "Aspect Arena" },
        { 919, "Sanctum Sprint" },
        { 920, "Southsun Survival" },
        { 922, "Labyrinthine Cliffs" },
        { 924, "Grandmaster of Om" },
        { 929, "The Crown Pavilion" },
        { 934, "Super Adventure Box" },
        { 935, "Super Adventure Box" },
        { 943, "Tower of Nightmares" },
        { 947, "Fractals of the Mists" },
        { 948, "Fractals of the Mists" },
        { 949, "Fractals of the Mists" },
        { 950, "Fractals of the Mists" },
        { 951, "Fractals of the Mists" },
        { 952, "Fractals of the Mists" },
        { 953, "Fractals of the Mists" },
        { 954, "Fractals of the Mists" },
        { 955, "Fractals of the Mists" },
        { 956, "Fractals of the Mists" },
        { 957, "Fractals of the Mists" },
        { 958, "Fractals of the Mists" },
        { 959, "Fractals of the Mists" },
        { 960, "Fractals of the Mists" },
        { 964, "Scarlet's Secret Lair" },
        { 968, "Edge of the Mists" },
        { 984, "Courtyard" },
        { 988, "Dry Top" },
        { 989, "Prosperity's Mystery" },
        { 990, "Cornered" },
        { 991, "Disturbance in Brisban Wildlands" },
        { 992, "Fallen Hopes" },
        { 993, "Scarlet's Secret Room" },
        { 994, "The Concordia Incident" },
        { 997, "Discovering Scarlet's Breakthrough" },
        { 998, "The Machine" },
        { 999, "Trouble at Fort Salma" },
        { 1000, "The Waypoint Conundrum" },
        { 1001, "Summit Invitations" },
        { 1002, "Mission Accomplished" },
        { 1003, "Rallying Call" },
        { 1004, "Plan of Attack" },
        { 1005, "Party Politics" },
        { 1006, "Foefire Cleansing" },
        { 1007, "Recalibrating the Waypoints" },
        { 1008, "The Ghosts of Fort Salma" },
        { 1009, "Taimi's Device" },
        { 1010, "The World Summit" },
        { 1011, "Battle of Champion's Dusk" },
        { 1015, "The Silverwastes" },
        { 1016, "Hidden Arcana" },
        { 1017, "Reunion with the Pact" },
        { 1018, "Caithe's Reconnaissance Squad" },
        { 1019, "Fort Trinity" },
        { 1021, "Into the Labyrinth" },
        { 1022, "Return to Camp Resolve" },
        { 1023, "Tracking the Aspect Masters" },
        { 1024, "No Refuge" },
        { 1025, "The Newly Awakened" },
        { 1026, "Meeting the Asura" },
        { 1027, "Pact Assaulted" },
        { 1028, "The Mystery Cave" },
        { 1029, "Arcana Obscura" },
        { 1032, "Prized Possessions" },
        { 1033, "Buried Insight" },
        { 1037, "The Jungle Provides" },
        { 1040, "Hearts and Minds" },
        { 1041, "Dragon's Stand" },
        { 1042, "Verdant Brink" },
        { 1043, "Auric Basin" },
        { 1045, "Tangled Depths" },
        { 1046, "Roots of Terror" },
        { 1048, "City of Hope" },
        { 1050, "Torn from the Sky" },
        { 1051, "Prisoners of the Dragon" },
        { 1052, "Verdant Brink" },
        { 1054, "Bitter Harvest" },
        { 1057, "Strange Observations" },
        { 1058, "Prologue: Rally to Maguuma" },
        { 1062, "Spirit Vale" },
        { 1063, "Southsun Crab Toss" },
        { 1064, "Claiming the Lost Precipice" },
        { 1065, "Angvar's Trove" },
        { 1067, "Angvar's Trove" },
        { 1068, "Gilded Hollow" },
        { 1069, "Lost Precipice" },
        { 1070, "Claiming the Lost Precipice" },
        { 1071, "Lost Precipice" },
        { 1072, "Southsun Crab Toss" },
        { 1073, "Guild Initiative Office" },
        { 1074, "Blightwater Shatterstrike" },
        { 1075, "Proxemics Lab" },
        { 1076, "Lost Precipice" },
        { 1078, "Claiming the Gilded Hollow" },
        { 1079, "Deep Trouble" },
        { 1080, "Branded for Termination" },
        { 1081, "Langmar Estate" },
        { 1082, "Langmar Estate" },
        { 1083, "Deep Trouble" },
        { 1084, "Southsun Crab Toss" },
        { 1086, "Save Our Supplies" },
        { 1087, "Proxemics Lab" },
        { 1088, "Claiming the Gilded Hollow" },
        { 1089, "Angvar's Trove" },
        { 1090, "Langmar Estate" },
        { 1091, "Save Our Supplies" },
        { 1092, "Scratch Sentry Defense" },
        { 1093, "Angvar's Trove" },
        { 1094, "Save Our Supplies" },
        { 1095, "Dragon's Stand (Heart of Thorns)" },
        { 1097, "Proxemics Lab" },
        { 1098, "Claiming the Gilded Hollow" },
        { 1099, " Desert Borderlands" },
        { 1100, "Scratch Sentry Defense" },
        { 1101, "Gilded Hollow" },
        { 1104, "Lost Precipice" },
        { 1105, "Langmar Estate" },
        { 1106, "Deep Trouble" },
        { 1107, "Gilded Hollow" },
        { 1108, "Gilded Hollow" },
        { 1109, "Angvar's Trove" },
        { 1110, "Scrap Rifle Field Test" },
        { 1111, "Scratch Sentry Defense" },
        { 1112, "Branded for Termination" },
        { 1113, "Scratch Sentry Defense" },
        { 1115, "Haywire Punch-o-Matic Battle" },
        { 1116, "Deep Trouble" },
        { 1117, "Claiming the Lost Precipice" },
        { 1118, "Save Our Supplies" },
        { 1121, "Gilded Hollow" },
        { 1122, "Claiming the Gilded Hollow" },
        { 1123, "Blightwater Shatterstrike" },
        { 1124, "Lost Precipice" },
        { 1126, "Southsun Crab Toss" },
        { 1128, "Scratch Sentry Defense" },
        { 1129, "Langmar Estate" },
        { 1130, "Deep Trouble" },
        { 1131, "Blightwater Shatterstrike" },
        { 1132, "Claiming the Lost Precipice" },
        { 1133, "Branded for Termination" },
        { 1134, "Blightwater Shatterstrike" },
        { 1135, "Branded for Termination" },
        { 1136, "Proxemics Lab" },
        { 1137, "Proxemics Lab" },
        { 1138, "Save Our Supplies" },
        { 1139, "Southsun Crab Toss" },
        { 1140, "Claiming the Lost Precipice" },
        { 1142, "Blightwater Shatterstrike" },
        { 1146, "Branded for Termination" },
        { 1147, "Spirit Vale" },
        { 1149, "Salvation Pass" },
        { 1153, "Tiger Den" },
        { 1154, "Special Forces Training Area" },
        { 1155, "Lion's Arch Aerodrome" },
        { 1156, "Stronghold of the Faithful" },
        { 1158, "Noble's Folly" },
        { 1159, "Research in Rata Novus" },
        { 1161, "Eir's Homestead" },
        { 1163, "Revenge of the Capricorn" },
        { 1164, "Fractals of the Mists" },
        { 1165, "Bloodstone Fen" },
        { 1166, "In Pursuit of Caudecus" },
        { 1167, "A Shadow's Deeds" },
        { 1169, "Rata Novus" },
        { 1170, "Taimi's Game" },
        { 1171, "Eternal Coliseum" },
        { 1172, "Dragon Vigil" },
        { 1173, "Taimi's Game" },
        { 1175, "Ember Bay" },
        { 1176, "Taimi's Game" },
        { 1177, "Fractals of the Mists" },
        { 1178, "Bitterfrost Frontier" },
        { 1180, "The Bitter Cold" },
        { 1181, "Frozen Out" },
        { 1182, "Precocious Aurene" },
        { 1185, "Lake Doric" },
        { 1188, "Bastion of the Penitent" },
        { 1189, "Regrouping with the Queen" },
        { 1190, "A Meeting of Ministers" },
        { 1191, "Beetlestone Manor" },
        { 1192, "The Second Vision" },
        { 1193, "The First Vision" },
        { 1194, "The Sword Regrown" },
        { 1195, "Draconis Mons" },
        { 1196, "Heart of the Volcano" },
        { 1198, "Taimi's Pet Project" },
        { 1200, "Hall of the Mists" },
        { 1201, "Asura Arena" },
        { 1202, "White Mantle Hideout" },
        { 1203, "Siren's Landing" },
        { 1204, "Palace Temple" },
        { 1205, "Fractals of the Mists" },
        { 1206, "Mistlock Sanctuary" },
        { 1207, "The Last Chance" },
        { 1208, "Shining Blade Headquarters" },
        { 1209, "The Sacrifice" },
        { 1210, "Crystal Oasis" },
        { 1211, "Desert Highlands" },
        { 1212, "Office of the Chief Councilor" },
        { 1214, "Windswept Haven" },
        { 1215, "Windswept Haven" },
        { 1217, "Sparking the Flame" },
        { 1219, "Enemy of My Enemy: The Beastmarshal" },
        { 1220, "Sparking the Flame (Prologue)" },
        { 1221, "The Way Forward" },
        { 1222, "Claiming Windswept Haven" },
        { 1223, "Small Victory (Epilogue)" },
        { 1224, "Windswept Haven" },
        { 1226, "The Desolation" },
        { 1227, "Hallowed Ground: Tomb of Primeval Kings" },
        { 1228, "Elon Riverlands" },
        { 1230, "Facing the Truth: The Sanctum" },
        { 1231, "Claiming Windswept Haven" },
        { 1232, "Windswept Haven" },
        { 1234, "To Kill a God" },
        { 1236, "Claiming Windswept Haven" },
        { 1240, "Blazing a Trail" },
        { 1241, "Night of Fires" },
        { 1242, "Zalambur's Office" },
        { 1243, "Windswept Haven" },
        { 1244, "Claiming Windswept Haven" },
        { 1245, "The Departing" },
        { 1246, "Captain Kiel's Office" },
        { 1247, "Enemy of My Enemy" },
        { 1248, "Domain of Vabbi" },
        { 1250, "Windswept Haven" },
        { 1252, "Crystalline Memories" },
        { 1253, "Beast of War" },
        { 1255, "Enemy of My Enemy: The Troopmarshal" },
        { 1256, "The Dark Library" },
        { 1257, "Spearmarshal's Lament" },
        { 1260, "Eye of the Brandstorm" },
        { 1263, "Domain of Istan" },
        { 1264, "Hall of Chains" },
        { 1265, "The Hero of Istan" },
        { 1266, "Cave of the Sunspear Champion" },
        { 1267, "Fractals of the Mists" },
        { 1268, "Fahranur, the First City" },
        { 1270, "Toypocalypse" },
        { 1271, "Sandswept Isles" },
        { 1274, "The Charge" },
        { 1275, "Courtyard" },
        { 1276, "The Test Subject" },
        { 1277, "The Charge" },
        { 1278, "???" },
        { 1279, "ERROR: SIGNAL LOST" },
        { 1281, "A Kindness Repaid" },
        { 1282, "Tracking the Scientist" },
        { 1283, "???" },
        { 1285, "???" },
        { 1288, "Domain of Kourna" },
        { 1289, "Seized" },
        { 1290, "Fractals of the Mists" },
        { 1291, "Forearmed Is Forewarned" },
        { 1292, "Be My Guest" },
        { 1294, "Sun's Refuge" },
        { 1295, "Legacy" },
        { 1296, "Storm Tracking" },
        { 1297, "A Shattered Nation" },
        { 1299, "Storm Tracking" },
        { 1300, "From the Ashes—The Deadeye" },
        { 1301, "Jahai Bluffs" },
        { 1302, "Storm Tracking" },
        { 1303, "Mythwright Gambit" },
        { 1304, "Mad King's Raceway" },
        { 1305, "Djinn's Dominion" },
        { 1306, "Secret Lair of the Snowmen (Squad)" },
        { 1308, "Scion & Champion" },
        { 1309, "Fractals of the Mists" },
        { 1310, "Thunderhead Peaks" },
        { 1313, "The Crystal Dragon" },
        { 1314, "The Crystal Blooms" },
        { 1315, "Armistice Bastion" },
        { 1316, "Mists Rift" },
        { 1317, "Dragonfall" },
        { 1318, "Dragonfall" },
        { 1319, "Descent" },
        { 1320, "The End" },
        { 1321, "Dragonflight" },
        { 1322, "Epilogue" },
        { 1323, "The Key of Ahdashim" },
        { 1326, "Dragon Bash Arena" },
        { 1327, "Dragon Arena Survival" },
        { 1328, "Auric Span" },
        { 1329, "Coming Home" },
        { 1330, "Grothmar Valley" },
        { 1331, "Strike Mission: Shiverpeaks Pass (Public)" },
        { 1332, "Strike Mission: Shiverpeaks Pass (Squad)" },
        { 1334, "Deeper and Deeper" },
        { 1336, "A Race to Arms" },
        { 1338, "Bad Blood" },
        { 1339, "Weekly Strike Mission: Boneskinner (Squad)" },
        { 1340, "Weekly Strike Mission: Voice of the Fallen and Claw of the Fallen (Public)" },
        { 1341, "Weekly Strike Mission: Fraenir of Jormag (Squad)" },
        { 1342, "The Invitation" },
        { 1343, "Bjora Marches" },
        { 1344, "Weekly Strike Mission: Fraenir of Jormag (Public)" },
        { 1345, "What's Left Behind" },
        { 1346, "Weekly Strike Mission: Voice of the Fallen and Claw of the Fallen (Squad)" },
        { 1349, "Silence" },
        { 1351, "Weekly Strike Mission: Boneskinner (Public)" },
        { 1352, "Secret Lair of the Snowmen (Public)" },
        { 1353, "Celestial Challenge" },
        { 1355, "Voice in the Deep" },
        { 1356, "Chasing Ghosts" },
        { 1357, "Strike Mission: Whisper of Jormag (Public)" },
        { 1358, "Eye of the North" },
        { 1359, "Strike Mission: Whisper of Jormag (Squad)" },
        { 1361, "The Nightmare Incarnate" },
        { 1362, "Forging Steel (Public)" },
        { 1363, "North Nolan Hatchery" },
        { 1364, "Cragstead" },
        { 1366, "Darkrime Delves" },
        { 1368, "Forging Steel (Squad)" },
        { 1369, "Canach's Lair" },
        { 1370, "Eye of the North" }
    };
    #endregion

    /// <summary>
    /// Generates a string that can be used to init the dictIdToMap.
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetMapIdToNameMappings()
    {
        string strOutput = "";
        for (int i = 1; i < 1500; i++)
        {
            System.Uri uriRequest = new System.Uri("https://api.guildwars2.com/v2/maps/" + i);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uriRequest))
            {
                //webRequest.SetRequestHeader("Authorization", "Authorization: Bearer " + _strApiKey);
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                    Debug.Log(": Error: " + webRequest.error);
                else
                {
                    if (webRequest.downloadHandler.isDone)
                    {
                        var data = (JObject)JsonConvert.DeserializeObject(webRequest.downloadHandler.text);
                        int iId = data["id"].Value<int>();
                        string strName = data["name"].Value<string>();
                        //dictIdToMap.Add(iId, strName);
                        Debug.Log(iId + ": " + strName);
                        strOutput += "\n{ " + iId + ", \"" + strName + "\" },";
                    }
                }
            }
        }

        Debug.Log(strOutput);
    }

    public IEnumerator GetAccountInfo(string _strApiKey, GW2APIAccountInfo _oGw2AccountAPI, CoroutineFeedback _finishedMarker)
    {
        //Debug.Log("Requesting account info.");

        if (!bCheckApiKey(_strApiKey))
            yield break;

        System.Uri uriRequest = new System.Uri("https://api.guildwars2.com/v2/account");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uriRequest))
        {
            webRequest.SetRequestHeader("Authorization", "Authorization: Bearer " + _strApiKey);
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                try
                {
                    _oGw2AccountAPI.init(webRequest.downloadHandler.text);
                }
                catch
                {
                    _finishedMarker.bFinished = true;
                    _finishedMarker.bSuccessfull = false;
                    _finishedMarker.exception = new Exception("key_not_valid");
                    yield break;
                }
            }
        }
        //Debug.Log("Received account info for: " + _oGw2AccountAPI.name);
        _finishedMarker.bSuccessfull = true;
        _finishedMarker.bFinished = true;
    }

    public IEnumerator GetCharacterInfo(string _strApiKey, GW2APICharInfos _oCharInfos, CoroutineFeedback _finishedMarker)
    {
        //Debug.Log("Requesting character info.");

        if (!bCheckApiKey(_strApiKey))
            yield break;

        System.Uri uriRequest = new System.Uri("https://api.guildwars2.com/v2/characters?ids=all");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uriRequest))
        {
            webRequest.SetRequestHeader("Authorization", "Authorization: Bearer " + _strApiKey);
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                try
                {
                    if (webRequest.downloadHandler.text.Contains("requires scope characters")) // it only says that the key would be invalid
                        throw new Exception("character scope missing");
                    _oCharInfos.init(webRequest.downloadHandler.text);
                }
                catch (Exception _ex)
                {
                    _finishedMarker.bFinished = true;
                    _finishedMarker.bSuccessfull = false;
                    if (_ex.Message == "character scope missing") // TODO: this string matching is kinda ugly
                        _finishedMarker.exception = new Exception("char_scope_missing");
                    else
                        _finishedMarker.exception = new Exception("key_not_valid");

                    yield break;
                }
            }
        }
        _finishedMarker.bSuccessfull = true;
        _finishedMarker.bFinished = true;
    }

    private bool bCheckApiKey(string _strApiKey)
    {
        if (_strApiKey.Length > 80) // small security measure
            return false;
        return true;
    }

    
}

public static class GW2Helpers
{
    public static string[] arFromDictList(object _oInput)
    {
        List<object> liInput = (List<object>)_oInput;

        string[] arOutput = new string[liInput.Count];
        for (int i = 0; i < arOutput.Length; i++)
        {
            arOutput[i] = liInput[i].ToString();
        }

        return arOutput;
    }

    public static int iFromDictObj(object _oInput)
    {
        int iOutput = 0;
        System.Int32.TryParse(_oInput.ToString(), out iOutput);

        return iOutput;
    }
}

public class GW2APIAccountInfo
{
    public string id;
    public string name;
    public int age;
    public int world;
    public string[] guilds;
    public string created;
    public string[] access;

    public GW2APIAccountInfo()
    { }

    public GW2APIAccountInfo (string _strJson)
    {
        init(_strJson);
    }

    public void init(string _strJson)
    {
        Dictionary<string, object> dictJson = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_strJson);

        id = dictJson["id"].ToString();
        name = dictJson["name"].ToString();
        age = GW2Helpers.iFromDictObj(dictJson["age"]);
        world = GW2Helpers.iFromDictObj(dictJson["world"]);
        guilds = GW2Helpers.arFromDictList(dictJson["guilds"]);
        created = dictJson["created"].ToString();
        access = GW2Helpers.arFromDictList(dictJson["access"]);
    }

    public bool bIsFreeToPlay()
    {
        return new List<string>(access).Find(x => x.Contains("PlayForFree")) != null;
    }
}

public class GW2APICharInfos
{
    public GW2APICharInfo[] arCharacterInfos;

    public GW2APICharInfos()
    { }

    public GW2APICharInfos(string _strJson)
    {
        init(_strJson);
    }

    public void init(string _strJson)
    {
        List<object> liJson = (List<object>)MiniJSON.Json.Deserialize(_strJson);
        arCharacterInfos = new GW2APICharInfo[liJson.Count];
        for (int i = 0; i < arCharacterInfos.Length; i++)
        {
            arCharacterInfos[i] = new GW2APICharInfo((Dictionary<string, object>)liJson[i]);
        }
    }
}

[Serializable]
public class GW2APICharInfo
{
    public enum Race { Asura, Charr, Human, Norn, Sylvari }
    public enum Gender { Male, Female }
    public enum Profession { Elementalist, Engineer, Guardian, Mesmer, Necromancer, Ranger, Revenant, Thief, Warrior }

    public string name = "";
    public Race race = Race.Asura;
    public Gender gender = Gender.Female;
    public Profession profession = Profession.Elementalist;
    public int level = -1;
    public string guild = "";
    public int age = -1;
    public string created = "";

    public GW2APICharInfo() { }

    public GW2APICharInfo(Dictionary<string, object> _dictJson)
    {
        name = _dictJson["name"].ToString();
        race = (Race)System.Enum.Parse(typeof(Race), _dictJson["race"].ToString());
        gender = (Gender)System.Enum.Parse(typeof(Gender), _dictJson["gender"].ToString());
        profession = (Profession)System.Enum.Parse(typeof(Profession), _dictJson["profession"].ToString());
        level = GW2Helpers.iFromDictObj(_dictJson["level"]);
        guild = _dictJson["guild"]?.ToString();
        age = GW2Helpers.iFromDictObj(_dictJson["age"]);
        created = _dictJson["created"].ToString();
    }

    public GW2APICharInfo(string _strName, Race _race, Gender _gender, Profession _profession, int _iLevel, string _strGuild, int _iAge, string _strCreated)
    {
        name = _strName;
        race = _race;
        gender = _gender;
        profession = _profession;
        level = _iLevel;
        guild = _strGuild;
        age = _iAge;
        created = _strCreated;
    }
}




