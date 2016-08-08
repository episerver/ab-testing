define({
    //Mock labels for unit tests
    addtestcommand: {
        label_text: 'add label',
        tooltip_text: 'add tooltip'
    },
    canceltestcommand: {
        label_text: 'cancel label',
        tooltip_text: 'cancel tooltip'
    },
    notificationbar: {
        ongoing_test: 'ongoing',
        version_in_test: "intest",
        scheduled_test: 'scheduled',
        completed_test: 'complete',
        details_link_text: 'detail link',
        details_link_tooltip: 'detail tooltip',
        winner_link_text: 'winner link',
        winner_link_tooltip: 'winner tooltip'
    },
    detailsview: {
        test_status_running: "Test is running, ",
        days_remaining: " day(s) remaining." ,
        started: "started ",
        test_status_completed: "Test completed, no go on and pick a winner...",
        test_status_not_started: "Test has not yet started, ",
        test_scheduled: "It is scheduled to begin ",
        by: "by"
    },
    pickwinnerview: {
        result_is_not_significant:"The results of this test are NOT significant.",
        result_is_significant: "The results of this test are significant.",
        early_pick_winner_message:"This test has not been completed, but you may pick a winner. Picking a winner now will end the test and publish the content chosen."
    }
});