﻿/* All CSS has been extracted and put into the "styles/twitterFeed.css" sheet */

new TWTR.Widget({
  version: 2,
  type: 'profile',
  rpp: 30,
  interval: 30000,
  width: 200,
  height: '100%; null:',
  theme: {
    shell: {
        background: 'null:',
        color: 'null:'
    },
    tweets: {
        background: 'null:',
        color: 'null:',
      links: 'null:'
    }
  },
  features: {
    scrollbar: true,
    loop: false,
    live: true,
    behavior: 'all'
  }
}).render().setUser('RaveRadarDev').start();