class_name Log


static func debug(message: String):
	print_debug(message)


static func info(message: String):
	print(message)


static func warning(message: String):
	push_warning(message)


static func error(message: String):
	push_error(message )


static func fatal(message: String):
	assert(false, message)
