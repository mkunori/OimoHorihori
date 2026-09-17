window.oimoShare = async function (text) {
    const url = window.location.href;

    if (navigator.share) {
        try {
            await navigator.share({
                title: "OIMO HORIHORI",
                text: text,
                url: url
            });

            return "shared";
        }
        catch (error) {
            if (error.name === "AbortError") {
                return "cancelled";
            }

            console.error(error);
        }
    }

    if (navigator.clipboard) {
        try {
            await navigator.clipboard.writeText(
                text + "\n" + url
            );

            return "copied";
        }
        catch (error) {
            console.error(error);
        }
    }

    return "failed";
};