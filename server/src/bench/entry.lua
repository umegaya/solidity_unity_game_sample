counter = 100
prefix = ""

wrk.method = "POST"
wrk.path = "/functions/entry"
wrk.headers = {
	["Content-Type"] = "application/json",
}

request = function()
	if counter == 100 then
		prefix = os.tmpname()
	end
	--io.write('prefix ', prefix, ' counter ', counter, '\n')
	--local uuid = os.tmpname() .. "-" .. os.tmpname() .. "-" .. os.tmpname()
	counter = counter + 1
	wrk.body   = '{"user_id":"'..prefix.."_"..counter..'","power":"'..math.random(20,30)..'"}'
	return wrk.format()
end
