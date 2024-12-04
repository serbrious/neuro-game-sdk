import express from "express";
import {json} from "body-parser";
import {WebSocket, WebSocketServer} from "ws";
import {JSONSchemaFaker} from "json-schema-faker";
import util from "util";

const app = express();
app.use(json());
app.listen(1337);

app.post("/", (req, res) => {
    send(req.body);
    res.sendStatus(200);
});

const wss = new WebSocketServer({port: 8000});

let connections: WebSocket[] = [];
let actions: Action[] = [];

wss.on("connection", function connection(ws) {
    console.log("+ Connection opened");
    connections.push(ws);
    ws.on("message", data => onMessageReceived(JSON.parse(data.toString()) as Message));
    ws.on("close", () => {
        console.log("- Connection closed");
        connections = connections.filter(c => c != ws);
    });

    send({command: "actions/reregister_all"});
});

async function onMessageReceived(message: Message) {
    console.log("<---", util.inspect(message, false, null, true));

    switch (message.command) {
        case "actions/register": {
            actions.push(...(message.data.actions as Action[]));
            break;
        }

        case "actions/unregister": {
            actions = actions.filter(a => !message.data.names.includes(a.name));
            break;
        }

        case "actions/force": {
            setTimeout(() => {
                const actionName: string = message.data.action_names[Math.floor(Math.random() * message.data.action_names.length)];

                if (actionName == "choose_name") {
                    send({command: "action", data: {id: message.data.id, name: "choose_name", data: JSON.stringify({name: "RANDY"})}});
                    return;
                }

                const action = actions.find(a => a.name === actionName);
                if (!action) return;

                const responseObj = !action?.schema ? undefined : JSON.stringify(JSONSchemaFaker.generate(action.schema));

                send({command: "action", data: {id: message.data.id, name: action.name, data: responseObj}});
            }, 500);
            break;
        }

        case "decision": {
            setTimeout(() => {
                const action: Action = message.data.actions[Math.floor(Math.random() * message.data.actions.length)];

                const responseObj = !action.schema ? undefined : JSON.stringify(JSONSchemaFaker.generate(action.schema));

                send({command: "action", data: {id: message.data.id, name: action.name, data: responseObj}});
            }, 2000);
            break;
        }
    }
}

export function send(msg: Message) {
    console.log("--->", util.inspect(msg, false, null, true));

    const msgStr = JSON.stringify(msg);
    for (const connection of connections) {
        connection.send(msgStr);
    }
}

type Message = {
    command: string,
    data?: { [key: string]: any }
}

type Action = {
    name: string,
    schema: any
}
