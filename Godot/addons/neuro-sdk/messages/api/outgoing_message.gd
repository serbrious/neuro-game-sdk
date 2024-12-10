class_name OutgoingMessage

func _get_command() -> String:
	Log.error("OutgoingMessage._get_command() is not implemented.")
	return "invalid"

func _get_data() -> Dictionary:
	return {}

func merge(_other: OutgoingMessage) -> bool:
	return false

func get_ws_message() -> WsMessage:
	return WsMessage.new(_get_command(), _get_data(), NeuroSdkConfig.game)
