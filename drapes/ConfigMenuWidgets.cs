using Gtk;
using Glade;

namespace Drapes {
    
    public partial class ConfigWindow {
    // These come from glade
        
        [Widget] Window             winPref;
        // Tab
        [Widget] Notebook           NtbConfig;
        // Things in the general tab
        [Widget] HScale             scaleTimer;
        [Widget] Button             btnClose;
        [Widget] CheckButton        cbtAutoStart;
        [Widget] CheckButton        cbtStartSwitch;
        [Widget] CheckButton        cbtMonitor;
        [Widget] FileChooserButton  fcbDir;
        // help
        [Widget] Button             btnHelp;
        // Add/Remove Style
        [Widget] Button             btnAdd;
        [Widget] Button             btnRemove;
        // Style box
        [Widget] ComboBox           cmbStyle;
        // The Treeview
        [Widget] TreeView           tvBgList;
    }
    
    
}
