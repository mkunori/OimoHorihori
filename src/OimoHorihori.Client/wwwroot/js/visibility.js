window.oimoVisibility = {
    handler: null,

    register: function (dotNetReference) {
        this.handler = function () {
            dotNetReference.invokeMethodAsync(
                "OnVisibilityChanged",
                document.hidden
            );
        };

        document.addEventListener(
            "visibilitychange",
            this.handler
        );
    },

    unregister: function () {
        if (this.handler) {
            document.removeEventListener(
                "visibilitychange",
                this.handler
            );

            this.handler = null;
        }
    }
};