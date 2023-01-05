using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows.Forms;

public class PinnedFiles : Form
{
    public enum fileGroups
    {
        songSelection,
        scoreScreen,
        modIcons,
        pauseScreen,
        failScreen,
        scoreBar,
        countdown,
        comboBurst,
        hitcircle,
        inputOverlay,
        approachCircle,
        followPoint,
        reverseArrow,
        sliderStartCircle,
        sliderEndCircle,
        sliderBall,
        spinner,
        hit0,
        hit50,
        hit100,
        hit300,
        hitLighting,
        pippidon,
        taiko,
        fruitCatcher,
        fruits,
        target,
    };

    Dictionary<fileGroups, string[]> pinnableFiles = new Dictionary<fileGroups, string[]>()
    {
        
        {fileGroups.songSelection, new string[]
        {
            "selection-mode",
            "selection-mode-over",
            "selection-mods",
            "selection-mods-over",
            "selection-random",
            "selection-random-over",
            "selection-options",
            "selection-options-over",
            "selection-tab",
            "button-left",
            "button-right",
            "button-middle",
            "menu-back",
            "mode-osu",
            "mode-taiko",
            "mode-fruits",
            "mode-mania",
            "mode-osu-med",
            "mode-taiko-med",
            "mode-fruits-med",
            "mode-mania-med",
            "mode-osu-small",
            "mode-taiko-small",
            "mode-fruits-small",
            "mode-mania-small",
            "menu-button-background",
        }},

        {fileGroups.modIcons, new string[]
        {
            "selection-mod-easy",
            "selection-mod-nofail",
            "selection-mod-halftime",
            "selection-mod-hardrock",
            "selection-mod-suddendeath",
            "selection-mod-perfect",
            "selection-mod-doubletime",
            "selection-mod-nightcore",
            "selection-mod-hidden",
            "selection-mod-fadein",
            "selection-mod-flashlight",
            "selection-mod-relax",
            "selection-mod-relax2",
            "selection-mod-spunout",
            "selection-mod-autoplay",
            "selection-mod-cinema",
            "selection-mod-scorev2",
            "selection-mod-key1",
            "selection-mod-key2",
            "selection-mod-key3",
            "selection-mod-key4",
            "selection-mod-key5",
            "selection-mod-key6",
            "selection-mod-key7",
            "selection-mod-key8",
            "selection-mod-key9",
            "selection-mod-keycoop",
            "selection-mod-mirror",
            "selection-mod-random",
            "selection-mod-touchdevice",
            "selection-mod-freemodallowed",
            "selection-mod-target",
        }},

        {fileGroups.scoreScreen, new string[]
        {
            "ranking-xh",
            "ranking-xh-small",
            "ranking-sh",
            "ranking-sh-small",
            "ranking-x",
            "ranking-x-small",
            "ranking-s",
            "ranking-s-small",
            "ranking-a",
            "ranking-a-small",
            "ranking-b",
            "ranking-b-small",
            "ranking-c",
            "ranking-c-small",
            "ranking-d",
            "ranking-d-small",
            "ranking-title",
            "ranking-panel",
            "ranking-maxcombo",
            "ranking-accuracy",
            "ranking-graph",
            "ranking-perfect",
            "ranking-winner",
            "ranking-replay",
            "ranking-retry",
        }},

        {fileGroups.pauseScreen, new string[]
        {
            "pause-overlay",
            "pause-back",
            "pause-continue",
            "pause-retry",
            "pause-replay",
        }},

        {fileGroups.failScreen, new string[]
        {
            "fail-background",
        }},
        
        {fileGroups.scoreBar, new string[]
        {
            "scorebar-bg",
            "scorebar-colour",
            "scorebar-colour-0",
            "scorebar-ki",
            "scorebar-kidanger",
            "scorebar-kidanger2",
            "scorebar-marker",
        }},
        
        {fileGroups.countdown, new string[]
        {
            "ready",
            "count3",
            "count2",
            "count1",
            "go",
        }},
        
        {fileGroups.comboBurst, new string[]
        {
            "comboburst",
            "comboburst-mania",
            "comboburst-fruits",
        }},
        
        {fileGroups.hitcircle, new string[]
        {
            "hitcircleselect",
            "hitcircle",
            "hitcircleoverlay",
        }},
        
        {fileGroups.inputOverlay, new string[]
        {
            "inputoverlay-background",
            "inputoverlay-key",
        }},

        {fileGroups.approachCircle, new string[]
        {        
            "approachcircle",
        }},
        
        {fileGroups.followPoint, new string[]
        {        
            "followpoint",
            "followpoint-0",
        }},
        
        {fileGroups.reverseArrow, new string[]
        {        
            "reversearrow",
        }},
        
        {fileGroups.sliderStartCircle, new string[]
        {        
            "sliderstartcircle",
            "sliderstartcircleoverlay",
        }},
        
        {fileGroups.sliderEndCircle, new string[]
        {        
            "sliderendcircle",
            "sliderendcircleoverlay",
        }},
        
        {fileGroups.sliderBall, new string[]
        {        
            "sliderfollowcircle",
            "sliderfollowcircle-0",
            "sliderb",
            "sliderb0",
            "sliderb-nd",
            "sliderb-spec",
        }},
        
        {fileGroups.spinner, new string[]
        {        
            "spinner-background",
            "spinner-metre",
            "spinner-bottom",
            "spinner-glow",
            "spinner-middle",
            "spinner-middle2",
            "spinner-top",
            "spinner-rpm",
            "spinner-clear",
            "spinner-spin",
            "spinner-osu",
            "spinner-warning",
            "spinner-circle",
            "spinner-approachcircle",
        }},
        
        {fileGroups.hit0, new string[]
        {        
            "hit0",
            "hit0-0",           
        }},
        
        {fileGroups.hit50, new string[]
        {        
            "hit50",
            "hit50-0",
        }},
        
        {fileGroups.hit100, new string[]
        {        
            "hit100",
            "hit100-0",
            "hit100k",
            "hit100k-0",
        }},
        
        {fileGroups.hit300, new string[]
        {        
            "hit300k",
            "hit300",
            "hit300-0",
            "hit300k-0",
            "hit300g",
            "hit300g-0",
        }},
        
        {fileGroups.hitLighting, new string[]
        {        
            "lighting",
        }},
        
        {fileGroups.pippidon, new string[]
        {        
            "pippidonidle",
            "pippidonkiai",
            "pippidonfail",
            "pippidonclear",
        }},
        
        {fileGroups.taiko, new string[]
        {        
            "taiko-flower-group",
            "taiko-slider",
            "taiko-slider-fail",
            "taiko-bar-left",
            "taiko-drum-inner",
            "taiko-drum-outer",
            "taiko-bar-right",
            "taiko-bar-right-glow",
            "taiko-barline",
            "taikohitcircle",
            "taikohitcircleoverlay",
            "taikobigcircle",
            "taikobigcircleoverlay",
            "taiko-glow",
            "taiko-roll-middle",
            "taiko-roll-end",
            "taiko-hit0",
            "taiko-hit100",
            "taiko-hit300",
            "taiko-hit100k",
            "taiko-hit300k",
            "taiko-hit300g",
        }},
        
        {fileGroups.fruitCatcher, new string[]
        {        
            "fruit-catcher-idle",
            "fruit-catcher-kiai",
            "fruit-catcher-fail",
            "fruit-ryuuta",
        }},
        
        {fileGroups.fruits, new string[]
        {        
            "fruit-apple",
            "fruit-apple-overlay",
            "fruit-grapes",
            "fruit-grapes-overlay",
            "fruit-orange",
            "fruit-orange-overlay",
            "fruit-pear",
            "fruit-pear-overlay",
            "fruit-bananas",
            "fruit-bananas-overlay",
            "fruit-drop",
            "fruit-drop-overlay",
        }},
        
        {fileGroups.target, new string[]
        {        
            "target",
            "targetoverlay",
            "target-pt-1",
            "target-pt-2",
            "target-pt-3",
            "target-pt-4",
            "target-pt-5",
            "targetoverlay-pt-1",
            "targetoverlay-pt-2",
            "targetoverlay-pt-3",
            "targetoverlay-pt-4",
            "targetoverlay-pt-5",
        }},


/*  "welcome_text",
        "menu-snow",
        "options-offset-tick",
        "cursor",
        "cursortrail",
        "cursormiddle",
        "cursor-smoke",
        "cursor-ripple",
        "star",
        "star2", */
        //"menu-back-0",
        //"play-skip-0",
        //"play-skip",
        /* "play-unranked",
        "play-warningarrow",
        "arrow-pause",
        "arrow-warning",
        "section-pass",
        "section-fail",
        "multi-skipped",
        "masking-border", */
        /* "particle50",
        "particle100",
        "particle300", */
        /* "sliderpoint10",
        "sliderpoint30", */
        
        
        //"sliderscorepoint",
    };

    public fileGroups[] GetGroups()
    {
        return pinnableFiles.Keys.ToArray();
    }
}