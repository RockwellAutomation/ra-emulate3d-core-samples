#An object which has a string property
class SimplePython:
	def __init__(self):
		self.mystring = 'Hello from Python Object'
		self.time = 0;
	def getMyString(self):
		return self.mystring
	def setMyString(self, newString):
		self.mystring = newString
	def getTime(self):
		return self.time
	def setTime(self, newTime):
		self.time = newTime

#return an instatiation of a simple object
def getSimpleObject():
	return SimplePython()