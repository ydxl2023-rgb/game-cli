
class XAccurateInterval {
    constructor(fn, delay, start, ...args) {
        this.fn = fn
        this.delay = delay
        this.args = args
        this.start = start || Date.now()
        this.expect = this.start + this.delay

        this.runNext(this.expect - Date.now())
    }

    runNext(delay) {
        this.timer = setTimeout(() => this.onTimeout(), delay)
    }

    stop() {
        clearTimeout(this.timer)
    }

    onTimeout() {
        try {
            this.fn(this, ...this.args)
        } catch (e) {
            log.fatal(e)
        }

        const msec = Date.now()
        this.expect += this.delay
        this.runNext(this.expect - msec)
    }
}

class XTime {
    msec = 0

    constructor(){
        this.msec = Date.now()
    }

    update(msec){
        this.msec = msec
    }

    now(){
        return this.msec
    }

    timestamp() {
        return Math.floor(this.now() / 1000)
    }

    toString(){
        const date = new Date(this.now())
        return date.toTimeString()
    }

    getDaySecs() {
        return 24 * 60 * 60
    }

    getDayStartMsec(offset, msec) {
        msec = (msec == undefined ? this.now() : msec)

        const date = new Date(msec + offset)
        date.setHours(0)
        date.setMinutes(0)
        date.setSeconds(0)
        date.setMilliseconds(0)

        return date.getTime() - offset
    }

    getDayStart(offset, sec) {
        return this.getDayStartMsec(offset * 1000, sec ? sec * 1000 : undefined) / 1000
    }

    getWeekStartMsec(offset, msec) {
        const date = new Date(msec + offset)

        date.setHours(0)
        date.setMinutes(0)
        date.setSeconds(0)
        date.setMilliseconds(0)
        date.setDate(date.getDate() - date.getDay() + (date.getDay() == 0 ? -6 : 1))
        return date.getTime() - offset
    }

    diffDaysMsec(offset, from, to) {
        const a = this.getDayStartMsec(offset, from)
        const b = this.getDayStartMsec(offset, to)
        return (b - a) / (this.getDaySecs() * 1000)
    }

    diffDays(offset, from, to) {
        return this.diffDaysMsec(offset * 1000, from * 1000, to * 1000)
    }

    setAccurateInterval(fn, delay, start, ...args) {
        return new XAccurateInterval(fn, delay, start, ...args)
    }

    getDayEnd(offset, sec) {
        return this.getDayStart(offset, sec) + this.getDaySecs()
    }
}

module.exports = new XTime()

