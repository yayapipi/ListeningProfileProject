mergeInto(LibraryManager.library, {
       SendGameResultToWeb: function(gameResultJsonString) {
           try {
               console.log("Unity call received: SendGameResultToWeb");
               var jsonString = UTF8ToString(gameResultJsonString);
               console.log("Data from Unity:", jsonString);
               
               var gameResult = JSON.parse(jsonString);
               
               if (window.handleGameComplete) {
                   window.handleGameComplete(gameResult);
               } else {
                   console.error("Frontend function 'handleGameComplete' not found on window object!");
               }
           } catch (e) {
               console.error("Error processing message from Unity in SendGameResultToWeb:", e);
           }
       }
   });