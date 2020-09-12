var WebURL = {
    OpenNewTab : function(url)
    {
        url = Pointer_stringify(url);
        window.open(url,'_blank');
    },
};
mergeInto(LibraryManager.library, WebURL);