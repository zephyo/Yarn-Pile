var WebIO = {
    SyncFiles : function()
    {
        FS.syncfs(false,function (err) {
            // handle callback
        });
    }
};

mergeInto(LibraryManager.library, WebIO);