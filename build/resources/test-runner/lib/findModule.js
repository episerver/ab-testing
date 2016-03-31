var fs = require("fs");
var path = require("path");

exports.findExecutable = function (name) {

    for (var i = 0; i < module.paths.length; i++) {
        var bin = path.join(module.paths[i], ".bin/" + name);
        if (process.platform === "win32") {
            bin += '.cmd';
        }
        if (fs.existsSync(bin)) {
            return bin;
        }
    }

    return null;
};
